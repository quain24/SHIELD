# SHIELD
### Aplikacja zarządzająca interaktywnym oprzyrządowaniem strzelnic

Shield będzie służył do kontrolowania wszystkich aktywnych urządzeń na każdej dostosowanej do potrzeb strzelnicy.
Aplikacja działa na zasadzie wymiany danych własnym protokołem (interfejs COM / FTDI) pomiędzy stacją bazową (pc) a jednym lub wieloma hubami bazującymi na dowolnym sprzęcie obsługującym .Net Framework

Każdy z podłączonych hubów nadzoruje dowolną liczbę konkretnych celów / wyświetlaczy / oświetlenia.

#### Główne zalety:
- Otwarty, czytelny i łatwo konfigurowalny protokół wymiany danych
- Dowolny zestaw reguł detekcji błędów
- Dowolna jednocześnie aktywna liczba hubów
- Łatwa rozszerzalność obsługiwanych peryferiów
- Komunikacja w czasie rzeczywistym z niskimi wymogami sprzętowymi dostosowana pod huby typu embedded system.

#### Użyte technologie:
Zadanie | Użyta technologia
-------------|-------------
DI / IoC | AutoFac
GUI | Caliburn.Micro
Wielowątkowość | TPL
Testy Jednostkowe | xUnit
SVN | Git
