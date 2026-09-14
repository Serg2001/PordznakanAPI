namespace PordznakanAPI.Enums
{
    public enum EPupilStatus
    {
        Օld,
        New,
        Repeater,
        Incomplete,
        Graduated,
        /// <summary>
        /// "hayt_admission" — supplied by data-api.emis.am. Appended so the value is not
        /// silently folded into New; existing members keep their stored numbers.
        /// </summary>
        HaytAdmission
    }
}
