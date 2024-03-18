namespace GIGXR.Platform.Scenarios.GigAssets.Validation
{
    public class IntegerPercentageAssetPropertyDataValidator// : IAssetPropertyDataValidator<byte>
    {
        public ValidatorEnums.ApplicationTimes ApplicationTime => ValidatorEnums.ApplicationTimes.Both;

        public IntegerPercentageAssetPropertyDataValidator()
        { 
        }

        public bool IsValid(byte data)
        {
            return data <= 100;
        }

        public byte ResolveInvalidValue(byte data)
        {
            if (data > 100)
            {
                return 100;
            }

            return data;
        }
    }
}
