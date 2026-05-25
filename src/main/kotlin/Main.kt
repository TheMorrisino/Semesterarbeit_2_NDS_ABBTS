//package Web
import java.lang.Thread.sleep

fun main() {
    var counter : Int = 0

    while (true) {
        println("Hello World ${counter}")
        counter++
        sleep(500)
        if (counter == 10)
            break
    }

    counter = 0

    while (true) {
        println("PEDRO Rakete startet in ${10 - counter}")
        counter++
        sleep(500)
        if (counter == 10)
            break
    }

    counter = 0

    println()

    println(" /\\/\\/\\                            /  \\\n" +
            "| \\  / |                         /      \\\n" +
            "|  \\/  |                       /          \\\n" +
            "|  /\\  |----------------------|     /\\     |\n" +
            "| /  \\ |                      |    /  \\    |\n" +
            "|/    \\|                      |   /    \\   |\n" +
            "|\\    /|                      |  | (  ) |  |\n" +
            "| \\  / |                      |  | (  ) |  |\n" +
            "|  \\/  |                 /\\   |  |      |  |   /\\\n" +
            "|  /\\  |                /  \\  |  |      |  |  /  \\\n" +
            "| /  \\ |               |----| |  |      |  | |----|\n" +
            "|/    \\|---------------|    | | /|   .  |\\ | |    |\n" +
            "|\\    /|               |    | /  |   .  |  \\ |    |\n" +
            "| \\  / |               |    /    |   .  |    \\    |\n" +
            "|  \\/  |               |  /      |   .  |      \\  |\n" +
            "|  /\\  |---------------|/        |   .  |        \\|\n" +
            "| /  \\ |              /   Space  |   .  |  X      \\\n" +
            "|/    \\|              (          |      |           )\n" +
            "|/\\/\\/\\|               |    | |--|      |--| |    |\n" +
            "------------------------/  \\-----/  \\/  \\-----/  \\--------\n" +
            "                        \\\\//     \\\\//\\\\//     \\\\//\n" +
            "                         \\/       \\/  \\/       \\/\n " +
            "" +
            "" +
            "" +
            "${counter}")


}