using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Q09
{
    public class Program
    {
        static string connStr = "Data Source=(local);Initial Catalog=DBforDevMA2;Integrated Security=true;";
        
        static void Main(string[] args)
        {
            int ownerId = EnterUser();
            Pet pet = SelectPet(ownerId);
            string reason = EnterReason();
            DateTime date = SelectDate();            
            BookConsultation(pet, reason, date);
        }

        private static void BookConsultation(Pet pet, string reason, DateTime date)
        {
            List<TimeSpan> timeSlots = GetAndPrintTimeSlots(date);

            Console.WriteLine("\nEnter the desired time slot number and press ENTER to complete the booking: ");

            int timeSlotIdx = -1;

            while (!int.TryParse(Console.ReadLine(), out timeSlotIdx) ||
                timeSlotIdx < 0 || timeSlotIdx > timeSlots.Count)
            {
                Console.WriteLine("You must enter a valid time slot number.");
            }

            DateTime starttime = date.Add(timeSlots[--timeSlotIdx]);
            DateTime endtime = starttime.AddMinutes(15);
            
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    using (SqlCommand command = new SqlCommand("sp_bookConsultation", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@pet", pet.Id);
                        command.Parameters.AddWithValue("@starttime", starttime);
                        command.Parameters.AddWithValue("@endtime", endtime);

                        var ownerdescription = new SqlParameter("@ownerdescription", SqlDbType.VarChar, 200);
                        ownerdescription.Value = reason;
                        command.Parameters.Add(ownerdescription);

                        conn.Open();
                        int inserted = command.ExecuteNonQuery();

                        if (inserted > 0)
                        {
                            Console.WriteLine("\nYour consultation was succesfully saved!");
                        }
                        else
                        {
                            Console.WriteLine("\nSomething went wrong and your consultation was not saved. Please try again.");
                        }

                        Console.WriteLine("Press any key to close the program.");
                        Console.ReadKey();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        private static List<TimeSpan> GetAndPrintTimeSlots(DateTime date)
        {
            HashSet<TimeSpan> bookedTimeslots = GetBookedTimeSlots(date);

            int slotsPerDay = (16 - 8) * 4;
            int idx = 0;
            TimeSpan t = new TimeSpan(8, 0, 0);
            var timeSlots = new List<TimeSpan>();

            Console.WriteLine($"\nAvailable time slots for {date.ToShortDateString()}:");

            for (int i = 0; i < slotsPerDay; i++)
            {
                if (bookedTimeslots.Contains(t))
                {
                    Console.WriteLine("-");
                    idx++;
                }
                else
                {
                    timeSlots.Add(t);
                    Console.WriteLine($"{i + 1 - idx}.\t{t}");
                }

                t = t.Add(new TimeSpan(0, 15, 0));
            }

            Console.WriteLine($"(Retrieved {DateTime.Now.ToShortDateString()} at {DateTime.Now.ToLongTimeString()}. Availability may change)");

            return timeSlots;
        }

        private static HashSet<TimeSpan> GetBookedTimeSlots(DateTime date)
        {
            var bookedTimeSlots = new HashSet<TimeSpan>();
            string query = "select starttime from consultation " + 
                "where datepart(yy, starttime) = @year " +
                "and datepart(m, starttime) = @month " +
                "and datepart(d, starttime) = @day";

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        conn.Open();

                        command.Parameters.Add(new SqlParameter("year", date.Year));
                        command.Parameters.Add(new SqlParameter("month", date.Month));
                        command.Parameters.Add(new SqlParameter("day", date.Day));

                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            bookedTimeSlots.Add(((DateTime)reader["starttime"]).TimeOfDay);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }

            return bookedTimeSlots;
        }

        private static DateTime SelectDate()
        {
            var date = new DateTime();
            bool isValidDate = false;

            Console.Write("\nPlease insert the desired date in the format 'd-M-yyyy': ");

            while (!isValidDate)
            {
                isValidDate = DateTime.TryParse(Console.ReadLine(), out date);

                if (!isValidDate)
                {
                    Console.WriteLine("The entered date was not recognized as a valid date.");
                    Console.Write("\nPlease insert the desired date in the format 'd-M-yyyy': ");
                }
                else if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    isValidDate = false;
                    Console.WriteLine("Sorry, the clinic is closed during the weekends. Please select a week day.");
                    Console.Write("\nPlease insert the desired date in the format 'd-M-yyyy': ");
                }
            }

            return date;
        }

        private static string EnterReason()
        {
            Console.WriteLine("What is the reason for the visit?");
            string reason = Console.ReadLine();

            return reason;
        }

        private static Pet SelectPet(int ownerId)
        {
            if (ownerId < 1)
            {
                throw new ArgumentException("The owner id must be larger than 0.");
            }

            Pet pet;
            List<Pet> pets;
            int petIdx;

            pets = GetPets(ownerId);
            
            if (pets.Count == 0)
            {
                Console.WriteLine("You have no pets registered. Press any key to close the program.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            Console.WriteLine("Select pet:");

            var petIndices = "";
            for (var i = 0; i < pets.Count; i++)
            {
                var j = i + 1;
                petIndices += j.ToString();
                Console.WriteLine($"{j}\t{pets[i].Name}");
            }

            Console.WriteLine();

            char c = ' ';
            while (!petIndices.Contains(c))
            {
                Console.Write("\b \b");
                c = char.ToLower(Console.ReadKey().KeyChar);
            }

            petIdx = (int)Char.GetNumericValue(c) - 1;
            pet = pets[petIdx];

            Console.WriteLine($"\nYou selected {pet.Name}\n");

            return pet;
        }

        private static List<Pet> GetPets(int ownerId)
        {
            var pets = new List<Pet>();
            string query = "select id, name, petowner from pet where petowner = @petowner";

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        command.Parameters.Add(new SqlParameter("petowner", ownerId));
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            var pet = new Pet();

                            pet.Id = (Int32)reader["id"];
                            pet.Name = (string)reader["name"];
                            pet.OwnerId = (Int32)reader["petowner"];

                            pets.Add(pet);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }

            return pets;
        }

        private static int EnterUser()
        {
            string userName = "";
            int userId = -1;

            while (userId == -1)
            {
                Console.Write("Please enter your user name: ");
                userName = Console.ReadLine();
                userId = GetOwnerId(userName);

                if (userId != -1)
                {
                    Console.WriteLine($"Welcome {userName}!\n");
                }
                else
                {
                    Console.WriteLine("The user name was not found in the database. Please try again.\n");
                }
            }

            return userId;
        }

        private static int GetOwnerId(string userName)
        {
            int ownerId = -1;
            string query = "select top 1 ownerid from petowner where name = @ownerName";

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        command.Parameters.Add(new SqlParameter("ownerName", userName));
                        ownerId = (int)(command.ExecuteScalar() ?? -1);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }

            return ownerId;
        }
    }
}
