namespace BE.Domain.Holiday;

// Všechny události, které mění zůstatek. Měsíční připisování (accrual) tu NENÍ – počítá se, neukládá.
public enum LedgerEntryType
{
    // Schválená dovolená byla vybrána (záporné hodiny).
    Usage,

    // Schválená dovolená byla zrušena, hodiny se vrací (kladné hodiny).
    CancellationReturn,

    // Nemoc přerušila dovolenou, hodiny se vrací (kladné hodiny).
    ObstacleReturn,

    // Hodiny převedené z tohoto roku do dalšího období (záporné hodiny).
    TransferOut,

    // Hodiny převedené z předchozího období (kladné hodiny).
    TransferIn,

    // Přepočet zůstatku po změně rozvrhu, aby zůstal stejný počet dní (+ nebo −).
    ScheduleConversion,

    // Hodiny nevybrané do termínu, které propadají (záporné hodiny).
    Expiry,

    // Hodiny předané mzdám k proplacení místo volna (záporné hodiny).
    PayoutFlag
}
