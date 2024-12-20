package com.yaskovdev;

import org.openjdk.jcstress.annotations.*;
import org.openjdk.jcstress.infra.results.II_Result;

@JCStressTest
@Outcome(id = "0, 0", expect = Expect.FORBIDDEN, desc = "Each thread reads one shared variable before writing to another.")
@Outcome(id = "0, 2", expect = Expect.ACCEPTABLE)
@Outcome(id = "1, 0", expect = Expect.ACCEPTABLE)
@Outcome(id = "1, 2", expect = Expect.ACCEPTABLE_INTERESTING)
@State
public class Question79288845Test {

    // Note: if you remove volatile, then "0, 0" will be possible.
    // You can also try without volatile but with both actor1 and actor2 synchronized. Then both the "0, 0" and "1, 2" will be impossible.
    private volatile int a = 0;
    private volatile int b = 0;

    @Actor
    public void actor1(II_Result r) {
        b = 1; // (1)
        r.r2 = a; // (2)
    }

    @Actor
    public void actor2(II_Result r) {
        a = 2; // (3)
        r.r1 = b; // (4)
    }
}
