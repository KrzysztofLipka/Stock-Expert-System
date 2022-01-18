# Stock-Expert-System
System sluży do prognozowania przyszłych kursów walut

# Elementy Systemu
## Moduł Data Serializer
Moduł ten jest odpowiedzialny za doawanie danych do bazy danych oraz ich aktualizację.
Aplikacja konsolowa przyjmuje dwa argumenty wejściowe: 
Pierwszy z nich ma wartość BulkLoad lub Update (Dodanie wszystkich istniejących rekordów do bazy danych lub pewien zakres danych)
Drugi to nazwa Społki dla ktorej mają byc pobrane dane, nazwa spółki musi być wcześcniej dodana w bazie danych w tabeli Companies.
[Link do funkcji main](https://github.com/KrzysztofLipka/Stock-Expert-System/blob/main/DataSerializer/Program.cs)


Dane pobrane sa z serwisu STOQQ w postaci pliku CSV.
Ponadto moduł słuzy do zapisania hisotrycznych transakcji (z poza funkcji min przez inne moduły).

## Moduł MachinleLearning

Moduł ten posiada Klasę służacą do wczytywania danych z bazy oraz Klasy treningowe dla algorytmów takich jak SSA ARIMA oraz SVM

Obecnie jedynym w pełni działającym modułdem jest SSA. Ze względu na różne sposoby doboru parametrów wejsciowych metoda SSA ma 3 klasy:


[SSAConstantParametersTrainer](https://github.com/KrzysztofLipka/Stock-Expert-System/blob/main/MachineLearning/Trainers/SSAConstantParametersTrainer.cs) - wybór stałych paemetrów które moga być wykorzystane w przypadku sezonowości.
[SSATrainSizeWindowSizeTrainer](https://github.com/KrzysztofLipka/Stock-Expert-System/blob/main/MachineLearning/Trainers/SSATrainSizeWindowSizeTrainer.cs) - iteracja trainSize oraz windowsSize w celu wybrania rozwiązania z niajmniejszym błędem na danych testowych.
[SSAWindowsSizeTrainer](https://github.com/KrzysztofLipka/Stock-Expert-System/blob/main/MachineLearning/Trainers/SSAWindowsSizeTrainer.cs) - iteracja windowsSize w celu wybrania rozwiązania z niajmniejszym błędem na danych testowych.

Powyższe 3 klasy dziedziczą z modułu [SSATrainerBase](https://github.com/KrzysztofLipka/Stock-Expert-System/blob/main/MachineLearning/Trainers/SSATrainerBase.cs)

## Moduł MachineLearning.Tests
Moduł do testów oraz generowania wykresów

## StockExpertSystemBackend
Backend Sytemu pozwalający na udostepnienie wyników prognozy oraz historycznych transakcji poprzez webApi.



## Interfejs użytkownika
Interfejs użytkonwika pozwalający [Link do folderu z kodem](https://github.com/KrzysztofLipka/Stock-Expert-System/tree/main/frontend) 


