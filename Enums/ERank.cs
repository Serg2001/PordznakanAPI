namespace PordznakanAPI.Enums
{
    /// <summary>
    /// Գիտական կոչում, Կտակում՝ academic_rank
    /// </summary>
    public enum ERank : int
    {
        Unknown = 0,  // տվյալ չկա
        Absence = 1,  // կոչում չունի
        Docent = 2,
        Professor = 3
    }
}