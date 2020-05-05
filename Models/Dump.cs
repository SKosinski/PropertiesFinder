using System.Collections.Generic;

namespace Models
{
    /// <summary>
    /// Klasa reprezentująca wynik jednego zrzutu danych ze strony.
    /// </summary>
    public class Dump : DumpDetails
    {
        /// <summary>
        /// Kolekcja reprezentująca wszystkie oferty znalezione podczas zrzutu. Te dane powinny zostać pobrane automatycznie
        /// za pomocą parsera.
        /// </summary>
        public IEnumerable<Entry> Entries { get; set; }
    }
}
