# Aplikacja webowa do wizualizacji oraz predykcji obciążenia systemu energetycznego

## Opis projektu
Aplikacja ma na celu wizualizację oraz predykcję obciążenia systemu energetycznego. Wizualizacja odbywa się poprzez wykresy zawierające rzeczywiste oraz przewidywane odciążenie. Predykcja natomiast jest realizowana przez rekurencyjną sieć neuronową LSTM.

## Założenia projektowe
- [x] Aplikacja webowa
- [x] Połączenie z lokalną bazą danych
- [x] Ciemny motyw graficzny, wraz z pomarańczowym kolorem bazowym
- [x] Użytkownicy: standardowy, administrator
- [x] Możliwość rejestracji wymagającej potwierdzenia konta przez email
- [x] Możliwość resetowania hasła
- [x] Autoryzacja oparta o ciasteczka
- [x] Zapamiętywanie zalogowanego użytkownika
- [x] Interaktywne wykresy
- [x] Własny zakres danych na wykresach
- [x] Pobieranie danych w formie pliku .CSV
- [x] Dokonywanie predykcji z poziomu ASP.NET Core
- [x] Panel administracyjny
- [x] Możliwość dodawania, edycji oraz usuwania danych z bazy
- [x] Możliwość zarządzania użytkownikami
- [x] Możliwość wysyłania wiadomości do administratora 

## Zastosowane technologie

1. Backend
   - ASP.NET Core 7.0
   - Entity Framework Core
   - Identity Framework

2. Frontend
   - Blazor
   - Bootstrap

3. Baza danych
   - MSSQL Server

4. Sieć neuronowa
   - Python
   - Pytorch
   - Eksport do formatu ONNX

## Użytkownicy

1. Użytkownik standardowy: może przeglądać dane oraz wykresy

2. Administrator: może dodawać, edytować oraz usuwać dane z bazy, uruchamiać predykcję, zarządzać użytkownikami i wiadomościami od nich.
