namespace stranddService.Models
{
    public class VehicleInfo
    {
        public string VehicleGUID { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }
        public string RegistrationNumber { get; set; }
        public string Description { get; set; }

        //Takes in VehicleGUID for Constructor
        public VehicleInfo(string vehicleGUID)
        {

            stranddContext context = new stranddContext();

            Vehicle returnVehicle = context.Vehicles.Find(vehicleGUID);

            if (returnVehicle == null)
            {
                this.VehicleGUID = "NO ASSOCIATED VEHICLE";
                this.RegistrationNumber = "NO ASSOCIATED VEHICLE";
                this.Description = "NO ASSOCIATED VEHICLE";
                
            }
            else
            {
                this.VehicleGUID = returnVehicle.Id;
                this.Make = returnVehicle.Make;
                this.Model = returnVehicle.Model;
                this.Year = returnVehicle.Year;
                this.Color = returnVehicle.Color;
                this.RegistrationNumber = returnVehicle.RegistrationNumber;
                this.Description = returnVehicle.Year + " " + returnVehicle.Color + " " + returnVehicle.Make + " " + returnVehicle.Model;
            }
        }
    }
}