import os

import keras_core as keras
import numpy as np
import torch

SEQ_LENGTH = 100
BATCH_SIZE = 512


def random_inputs_and_targets(songs, seq_length, batch_size):
    random_start_indexes = np.random.choice(songs.shape[0] - seq_length - 1, batch_size)
    x_batch = np.reshape([songs[i:i + seq_length] for i in random_start_indexes], [batch_size, seq_length])
    y_batch = np.reshape([songs[i + 1:i + seq_length + 1] for i in random_start_indexes], [batch_size, seq_length])
    return x_batch, y_batch


def generate_songs(model, char_to_index, index_to_char, start_string, generation_length=1000):
    input_eval = [char_to_index[s] for s in start_string]
    input_eval = torch.unsqueeze(torch.tensor(input_eval), 0)

    text_generated = []

    for i in range(generation_length):
        predictions = torch.squeeze(model(input_eval), 0)
        # The problem was here: it was "torch.nn.functional.softmax(predictions, dim=0)".
        # It worked when I changed it to "predictions.exp()". I thought it was because the model output was log softmax, so exp of the output is the softmax.
        # However, the model output is simply logits, so using "torch.nn.functional.softmax" was correct, but the "dim" was incorrect, it should be -1, not 0.
        # If the dim is 0, then the predictions turn into all ones, which if of course wrong.
        predicted_index = torch.multinomial(torch.nn.functional.softmax(predictions, dim=-1), 1, replacement=True)[-1, 0]
        input_eval = torch.unsqueeze(torch.unsqueeze(predicted_index, 0), 0)
        text_generated.append(index_to_char[predicted_index.item()])

    return start_string + "".join(text_generated)


def build_model(batch_size, stateful):
    return keras.Sequential([
        keras.layers.Input(shape=(SEQ_LENGTH,), batch_size=batch_size),
        keras.layers.Embedding(len(vocabulary), 256),
        keras.layers.LSTM(1024, return_sequences=True, stateful=stateful),
        keras.layers.Dense(len(vocabulary), activation=None)  # activation is None by default, but other options like "log_softmax" are possible
    ])


if __name__ == "__main__":
    cwd = os.path.dirname(__file__)
    with open(os.path.join(cwd, "songs.txt"), "r") as f:
        songs = f.read()

    vocabulary = sorted(set(songs))
    index_to_char, char_to_index = np.array(vocabulary), {u: i for i, u in enumerate(vocabulary)}
    vectorized_songs = np.array([char_to_index[character] for character in songs])

    model = build_model(BATCH_SIZE, False)
    model.summary()

    loss_function = torch.nn.CrossEntropyLoss()
    optimizer = torch.optim.Adam(model.parameters(), lr=5e-3)
    for i in range(1000):
        inputs, targets = random_inputs_and_targets(vectorized_songs, seq_length=SEQ_LENGTH, batch_size=BATCH_SIZE)

        predictions = model(inputs)
        loss = loss_function(predictions.permute(0, 2, 1), torch.from_numpy(targets).long().cuda())

        loss.backward()
        optimizer.step()
        optimizer.zero_grad()
        if i % 10 == 0:
            print("Loss at iteration", i, "is", loss.item())

    torch.save(model.state_dict(), os.path.join(cwd, "model.pt"))
    print("The model has been saved")

    trained_model = build_model(1, True)
    trained_model.load_state_dict(torch.load(os.path.join(cwd, "model.pt")))
    trained_model.eval()

    print("Generated songs:", generate_songs(trained_model, char_to_index, index_to_char, start_string="X"))
