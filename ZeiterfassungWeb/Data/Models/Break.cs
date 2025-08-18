namespace ZeiterfassungWeb.Data.Models
{
    public class Break : TimeBlock
    {
        public override bool IsWork => false;
    }
}
