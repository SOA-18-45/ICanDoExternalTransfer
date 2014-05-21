ICanDoExternalTransfer
======================

<b>Autorzy:</b>
- Bartłomiej Wojdan
- Michał Kidawa

<b>Opis:</b>
Serwis oferuje wykonanie przelewu zewnętrznego.
Oferuje dwie funkcjonalności i jeden typ (DataContract) <i>Transfer</i>.

//Wykonanie transferu. Jako parametry podajemy Guid klienta, który wysyła oraz do którego przelew ma być wykonanie oraz kwotę przelewu.
1) bool TransferMoney(Guid clientID, Guid externalClientID, double amount);

//Pobranie historii przelewów dla klienta
2) Transfer[] GetPreviousTransfers();
