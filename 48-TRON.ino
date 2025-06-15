// Pines de botones
const int botonUp    = 2;
const int botonDown  = 3;
const int botonLeft  = 4;
const int botonRight = 5;

// Pines de LEDs
const int ledUp    = 7;
const int ledDown  = 6;
const int ledLeft  = 8;
const int ledRight = 9;

void setup() {
  Serial.begin(115200);

  pinMode(botonUp,    INPUT_PULLUP);
  pinMode(botonDown,  INPUT_PULLUP);
  pinMode(botonLeft,  INPUT_PULLUP);
  pinMode(botonRight, INPUT_PULLUP);

  pinMode(ledUp,    OUTPUT);
  pinMode(ledDown,  OUTPUT);
  pinMode(ledLeft,  OUTPUT);
  pinMode(ledRight, OUTPUT);
}

void loop() {
  bool bUp    = digitalRead(botonUp)    == LOW;
  bool bDown  = digitalRead(botonDown)  == LOW;
  bool bLeft  = digitalRead(botonLeft)  == LOW;
  bool bRight = digitalRead(botonRight) == LOW;

  // Botón UP → Boost
  if (bUp) {
    Serial.println("U");
    digitalWrite(ledUp, HIGH);
  } else {
    digitalWrite(ledUp, LOW);
  }

  // Botón DOWN → Trail
  if (bDown) {
    Serial.println("D");
    digitalWrite(ledDown, HIGH);
  } else {
    digitalWrite(ledDown, LOW);
  }

  // Botón LEFT → Giro Izquierda
  if (bLeft) {
    Serial.println("L");
    digitalWrite(ledLeft, HIGH);
  } else {
    digitalWrite(ledLeft, LOW);
  }

  // Botón RIGHT → Giro Derecha
  if (bRight) {
    Serial.println("R");
    digitalWrite(ledRight, HIGH);
  } else {
    digitalWrite(ledRight, LOW);
  }

  delay(30);  // Controla la tasa de envío
}

