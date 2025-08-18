namespace ZeiterfassungWeb.Data.Models
{
    public class Work : TimeBlock
    {
        public override bool IsWork => true;
    }
}
