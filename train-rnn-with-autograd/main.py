import os

import keras_core as keras
import numpy as np
import torch

SEQ_LENGTH = 100
BATCH_SIZE = 64


def random_inputs_and_targets(songs, seq_length, batch_size):
    random_start_indexes = np.random.choice(songs.shape[0] - seq_length - 1, batch_size)
    x_batch = np.reshape([songs[i:i + seq_length] for i in random_start_indexes], [batch_size, seq_length])
    y_batch = np.reshape([songs[i + 1:i + seq_length + 1] for i in random_start_indexes], [batch_size, seq_length])
    return x_batch, y_batch


if __name__ == '__main__':
    with open(os.path.join(os.path.dirname(__file__), "songs.txt"), "r") as f:
        songs = f.read()

    vocabulary = sorted(set(songs))
    index_to_char, char_to_index = np.array(vocabulary), {u: i for i, u in enumerate(vocabulary)}
    vectorized_songs = np.array([char_to_index[character] for character in songs])

    model = keras.Sequential([
        keras.layers.Input(shape=(SEQ_LENGTH,), batch_size=BATCH_SIZE),
        keras.layers.Embedding(len(vocabulary), 256),
        keras.layers.LSTM(1024, return_sequences=True, stateful=True),
        keras.layers.Dense(len(vocabulary))
    ])
    model.summary()

    torch.autograd.set_detect_anomaly(True)
    loss_fn = torch.nn.CrossEntropyLoss()
    optimizer = torch.optim.Adam(model.parameters(), lr=5e-3)
    for i in range(500):
        inputs, targets = random_inputs_and_targets(vectorized_songs, seq_length=SEQ_LENGTH, batch_size=BATCH_SIZE)

        predictions = model(inputs)
        loss = loss_fn(predictions.permute((0, 2, 1)), torch.from_numpy(targets).long())

        loss.backward()
        optimizer.step()
        optimizer.zero_grad()
        print("Loss at iteration", i, "is", loss.item())

    print("The model has been trained")
