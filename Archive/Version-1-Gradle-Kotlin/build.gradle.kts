plugins {
    kotlin("jvm") version "2.3.20"
    application
}

group = "Web-App-Ressourcix"
version = "1.0-SNAPSHOT"

repositories {
    mavenCentral()
}

dependencies {
    testImplementation(kotlin("test"))
}

kotlin {
    jvmToolchain(21)
}

tasks.test {
    useJUnitPlatform()
}

tasks.withType<Jar> {
    manifest {
        attributes["Main-Class"] = "MainKt"
    }
}

tasks.withType<Test> {
    useJUnitPlatform()
}

application {
    mainClass.set("MainKt")
}