using System.Collections.Generic;
using IMK_web.Models;
using Newtonsoft.Json;

namespace IMK_web.Data
{
    public class Seed
    {
        private readonly DataContext dataContext;
        public Seed(DataContext dataContext)
        {
            this.dataContext=dataContext;

        }
        public void seedUsers (){
            var userData = System.IO.File.ReadAllText("Data/users.json");
            var users = JsonConvert.DeserializeObject<List<User>>(userData);
            foreach(User user in users){
                dataContext.Users.Add(user);
            }
             dataContext.SaveChanges();
        }
        public void seedCountries(){
            var countriesData = System.IO.File.ReadAllText("Data/countries.json");
            var countries = JsonConvert.DeserializeObject<List<Country>>(countriesData);
            foreach(Country country in countries){
                dataContext.Countries.Add(country);
            }
             dataContext.SaveChanges();
        }

    }
}