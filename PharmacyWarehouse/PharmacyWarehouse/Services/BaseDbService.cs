using PharmacyWarehouse.Data;

namespace PharmacyWarehouse.Services
{
    public class BaseDbService
    {
        private BaseDbService() 
        {
            context = new PharmacyWarehouseContext();
        }

        private static BaseDbService? instance;

        public static BaseDbService Instance
        {
            get
            {
                if (instance == null) 
                    instance = new BaseDbService();
                return instance;
            }
        }
        private static SystemInfoService? _systemInfo;
        public static SystemInfoService SystemInfo
        {
            get
            {
                if (_systemInfo == null)
                    _systemInfo = new SystemInfoService();
                return _systemInfo;
            }
        }


        private PharmacyWarehouseContext context;

        public PharmacyWarehouseContext Context => context;

    }
}
