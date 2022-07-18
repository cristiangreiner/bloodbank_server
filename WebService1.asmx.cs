using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using System.Data.SqlClient;

namespace bloodbank_server
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        [WebMethod]
        private string getDBconnectionString()
        {
            return @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=D:\AN III\II\PROJECT\bloodbank_server\App_Data\bloodbank_db.mdf";
        }


        [WebMethod]
        public string login(string username, string password)
        {
            DataSet dsUsers = new DataSet();
            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            string query = "SELECT * FROM Users WHERE Username='" + username + "'";
            SqlDataAdapter adapter = new SqlDataAdapter(query, myCon);
            adapter.Fill(dsUsers, "Users");
            myCon.Close();
            try
            {
                DataRow dr = dsUsers.Tables["Users"].Rows[0];
                String saved_password = dr.ItemArray.GetValue(1).ToString();
                if (password.Equals(saved_password)) return dr.ItemArray.GetValue(2).ToString(); ; //correct password, return the user type
                return "false"; //wrong password
            }
            catch { myCon.Close(); return "false"; }//no such username found 
        }

        [WebMethod]
        public string addUser(string newUsername, string newPassword, string newType, string doctorSecretCode, string donorCNP, string donorDOB, string firstName, string lastName)
        {

            if (newUsername == "" || newPassword == "") return "Missing data. Fill all forms!";

            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            myCon.Open();
            string query = "INSERT INTO Users (username, password, type) VALUES ('" + newUsername + "','" + newPassword + "','" + newType + "')";
            SqlCommand insertNewUser = new SqlCommand(query, myCon);
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                adapter.UpdateCommand = insertNewUser;
                adapter.UpdateCommand.ExecuteNonQuery();
                insertNewUser.Dispose();
                myCon.Close();
                if (newType == "doctor")
                {
                    string result = addDoctor(doctorSecretCode, newUsername, firstName, lastName);
                    if (!result.Equals("Account created successfully"))
                        deleteUser(newUsername); // the user was already created but the doctor creation failed
                    return result;
                }
                else if (newType == "donor")
                { 
                    string result = addDonor(donorCNP, newUsername, firstName, lastName, donorDOB);
                    if (!result.Equals("Account created successfully")) deleteUser(newUsername); // the user was already created but the donor creation failed
                    return result;
                } 
                return "Invalid user type";
            }
            catch { myCon.Close(); return "Username already exists!"; }

        }

        [WebMethod]
        private string addDoctor(string secretCode, string newUsername, string firstName, string lastName)
        {   //ONLY TO BE CALLED INSIDE AddUser

            if (secretCode == "" || firstName == "" || lastName == "") return "Missing data. Fill all forms!";

            if (secretCode != "doc123") return "Could not verify doctor identity!"; // a very secure password

            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            myCon.Open();
            var rand = new Random(); //for doc id
            string ID_doc = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + rand.Next(10000, 99999);
            string query = "INSERT INTO Doctors (ID_doc, username, first_name, last_name, ID_center) VALUES ('" + ID_doc + "','" + newUsername + "','"
                                                                                                   + firstName + "','" + lastName + "','" + -1 + "')";
            SqlCommand insertNewDoctor = new SqlCommand(query, myCon);
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                adapter.UpdateCommand = insertNewDoctor;
                adapter.UpdateCommand.ExecuteNonQuery();
                insertNewDoctor.Dispose();
                myCon.Close();
                return "Account created successfully";
            }
            catch { myCon.Close(); return "Something went wrong. Please try again!"; }

        }


        [WebMethod]
        private string addDonor(string CNP, string newUsername, string firstName, string lastName, string DOB)
        {   //ONLY TO BE CALLED INSIDE AddUser

            if (CNP == "" || firstName == "" || lastName == "" || DOB == "") return "Missing data. Fill all forms!";

            DateTime dt = DateTime.Now;
            TimeSpan ts = dt - DateTime.Parse(DOB).Date;
            if (ts.Days < 6575) return "You must be 18 years old to have an account!";

            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            myCon.Open();
            var rand = new Random(); //for donor id
            string ID_don = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + rand.Next(10000, 99999);
            string query = "INSERT INTO Donors (ID_donor, CNP, username, first_name, last_name, DOB) VALUES ('" + ID_don + "','" + CNP + "','" + newUsername + "','"
                                                                                                             + firstName + "','" + lastName + "','" + DOB + "')";
            SqlCommand insertNewDonor = new SqlCommand(query, myCon);
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                adapter.UpdateCommand = insertNewDonor;
                adapter.UpdateCommand.ExecuteNonQuery();
                insertNewDonor.Dispose();
                myCon.Close();
                return "Account created successfully";
            }
            catch { myCon.Close(); return "Something went wrong. Please try again!"; }

        }

        [WebMethod]
        public void deleteUser(string username)
        {
            //should only be accesible once logged inside account; no try/catch required
            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            myCon.Open();
            string query = "DELETE FROM Users WHERE username = '" + username + "'";
            SqlCommand delUser = new SqlCommand(query, myCon);
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.UpdateCommand = delUser;
            adapter.UpdateCommand.ExecuteNonQuery();
            myCon.Close();
        }

        [WebMethod]
        public string updateUserUsername(string username, string newUsername)
        {
            if (newUsername == "") return "Missing data. Fill all forms!";

            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            myCon.Open();
            string query = "UPDATE Users SET username = '" + newUsername + "' WHERE Username = '" + username + "'";
            SqlCommand updateUser = new SqlCommand(query, myCon);
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                adapter.UpdateCommand = updateUser;
                adapter.UpdateCommand.ExecuteNonQuery();
                updateUser.Dispose();
                myCon.Close();
                return "Account updated successfully";
            }
            catch { myCon.Close(); return "Username already exists!"; }
        }

        [WebMethod]
        public string updateUserPassword(string username, string newPassword)
        {
            if (newPassword == "") return "Missing data. Fill all forms!";

            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            myCon.Open();
            string query = "UPDATE Users SET password = '" + newPassword + "' WHERE Username = '" + username + "'";
            SqlCommand updateUser = new SqlCommand(query, myCon);
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.UpdateCommand = updateUser;
            adapter.UpdateCommand.ExecuteNonQuery();
            updateUser.Dispose();
            myCon.Close();
            return "Account updated successfully";
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////   DONORS    //////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [WebMethod]
        public string getDonorID(string username)
        {
            DataSet dsUsers = new DataSet();
            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            string query = "SELECT * FROM Donors WHERE Username='" + username + "'";
            SqlDataAdapter adapter = new SqlDataAdapter(query, myCon);
            adapter.Fill(dsUsers, "Donors");
            myCon.Close();
            try
            {
                DataRow dr = dsUsers.Tables["Donors"].Rows[0];
                return dr.ItemArray.GetValue(0).ToString();
            }
            catch { myCon.Close(); return "Error loading data"; }//no such username found 
        }

        [WebMethod]
        public string getDonorCNP(string ID_donor)
        {
            DataSet dsUsers = new DataSet();
            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            string query = "SELECT * FROM Donors WHERE ID_donor='" + ID_donor + "'";
            SqlDataAdapter adapter = new SqlDataAdapter(query, myCon);
            adapter.Fill(dsUsers, "Donors");
            myCon.Close();
            try
            {
                DataRow dr = dsUsers.Tables["Donors"].Rows[0];
                return dr.ItemArray.GetValue(1).ToString(); 
            }
            catch { myCon.Close(); return "Error loading data"; }//no such username found 
        }

        [WebMethod]
        public string getDonorFirstName(string ID_donor)
        {
            DataSet dsUsers = new DataSet();
            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            string query = "SELECT * FROM Donors WHERE ID_donor='" + ID_donor + "'";
            SqlDataAdapter adapter = new SqlDataAdapter(query, myCon);
            adapter.Fill(dsUsers, "Donors");
            myCon.Close();
            try
            {
                DataRow dr = dsUsers.Tables["Donors"].Rows[0];
                return dr.ItemArray.GetValue(3).ToString(); 
            }
            catch { myCon.Close(); return "Error loading data"; }//no such username found 
        }

        [WebMethod]
        public string getDonorLastName(string ID_donor)
        {
            DataSet dsUsers = new DataSet();
            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            string query = "SELECT * FROM Donors WHERE ID_donor='" + ID_donor + "'";
            SqlDataAdapter adapter = new SqlDataAdapter(query, myCon);
            adapter.Fill(dsUsers, "Donors");
            myCon.Close();
            try
            {
                DataRow dr = dsUsers.Tables["Donors"].Rows[0];
                return dr.ItemArray.GetValue(4).ToString(); 
            }
            catch { myCon.Close(); return "Error loading data"; }//no such username found 
        }

        [WebMethod]
        public string getDonorDOB(string ID_donor)
        {
            DataSet dsUsers = new DataSet();
            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            string query = "SELECT * FROM Donors WHERE ID_donor='" + ID_donor + "'";
            SqlDataAdapter adapter = new SqlDataAdapter(query, myCon);
            adapter.Fill(dsUsers, "Donors");
            myCon.Close();
            try
            {
                DataRow dr = dsUsers.Tables["Donors"].Rows[0];
                // the data is imported as DateTime with unneceseary time as 00:00:00, we want to extract the date:
                DateTime date = new DateTime();
                date = DateTime.Parse(dr.ItemArray.GetValue(5).ToString()); 
                return date.ToString("d");
            }
            catch { myCon.Close(); return "Error loading data"; }//no such username found 
        }

        [WebMethod]
        public string updateDonorDetails(string ID_donor, string newCNP, string newFirstName, string newLastName, string newDOBshortFormat)
        {
            //ONLY TO BE USED INSIDE DONOR ACCOUNT
            
            if (newCNP == "" || newFirstName == "" || newLastName == "" || newDOBshortFormat == "") return "Missing data. Fill all forms!";

            DateTime dt = DateTime.Now;
            TimeSpan ts = dt - DateTime.Parse(newDOBshortFormat).Date;
            if (ts.Days < 6575) return "You must be 18 years old to have an account!";

            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            myCon.Open();
            string query = "UPDATE Donors SET CNP = '" + newCNP + "', first_name = '" + newFirstName + "', last_name = '" + newLastName + "', dob = '" + newDOBshortFormat + "' " +
                                 "WHERE ID_donor = '" + ID_donor + "'";
            SqlCommand updateUser = new SqlCommand(query, myCon);
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                adapter.UpdateCommand = updateUser;
                adapter.UpdateCommand.ExecuteNonQuery();
                updateUser.Dispose();
                myCon.Close();
                return "Account updated successfully";
            }
            catch { myCon.Close(); return "Something went wrong. Please try again!"; }
        }


        //////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////       DOCTORS       /////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////



        [WebMethod]
        public string getDoctorID(string username)
        {
            DataSet dsUsers = new DataSet();
            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            string query = "SELECT * FROM Doctors WHERE Username='" + username + "'";
            SqlDataAdapter adapter = new SqlDataAdapter(query, myCon);
            adapter.Fill(dsUsers, "Doctors");
            myCon.Close();
            try
            {
                DataRow dr = dsUsers.Tables["Doctors"].Rows[0];
                return dr.ItemArray.GetValue(0).ToString();
            }
            catch { myCon.Close(); return "Error loading data"; }//no such username found 
        }

        [WebMethod]
        public string getDoctorFirstName(string ID_doc)
        {
            DataSet dsUsers = new DataSet();
            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            string query = "SELECT * FROM Doctors WHERE ID_doc='" + ID_doc + "'";
            SqlDataAdapter adapter = new SqlDataAdapter(query, myCon);
            adapter.Fill(dsUsers, "Doctors");
            myCon.Close();
            try
            {
                DataRow dr = dsUsers.Tables["Doctors"].Rows[0];
                return dr.ItemArray.GetValue(2).ToString();
            }
            catch { myCon.Close(); return "Error loading data"; }//no such username found 
        }

        [WebMethod]
        public string getDoctorLastName(string ID_doc)
        {
            DataSet dsUsers = new DataSet();
            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            string query = "SELECT * FROM Doctors WHERE ID_doc='" + ID_doc + "'";
            SqlDataAdapter adapter = new SqlDataAdapter(query, myCon);
            adapter.Fill(dsUsers, "Doctors");
            myCon.Close();
            try
            {
                DataRow dr = dsUsers.Tables["Doctors"].Rows[0];
                return dr.ItemArray.GetValue(3).ToString();
            }
            catch { myCon.Close(); return "Error loading data"; }//no such username found 
        }

        [WebMethod]
        public string getDoctorCenterID(string ID_doc)
        {
            DataSet dsUsers = new DataSet();
            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            string query = "SELECT * FROM Doctors WHERE ID_doc='" + ID_doc + "'";
            SqlDataAdapter adapter = new SqlDataAdapter(query, myCon);
            adapter.Fill(dsUsers, "Doctors");
            myCon.Close();
            try
            {
                DataRow dr = dsUsers.Tables["Doctors"].Rows[0];
                string id_center = dr.ItemArray.GetValue(4).ToString();
                if (id_center == "-1") return "No center selected";
                return dr.ItemArray.GetValue(4).ToString();
            }
            catch { myCon.Close(); return "Error loading data"; }//no such username found 
        }

        [WebMethod]
        public string updateDoctorDetails(string ID_doc, string newFirstName, string newLastName, int newCenterID)
        {
            // ONLY TO BE USED INSIDE DOCTOR ACCOUNT
            
            if (newFirstName == "" || newLastName == "") return "Missing data. Fill all forms!";

            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            myCon.Open();
            string query = "UPDATE Doctors SET first_name = '" + newFirstName + "', last_name = '" + newLastName + "', ID_center = '" + newCenterID + "' " +
                                "WHERE ID_doc = '" + ID_doc + "'";

            SqlCommand updateUser = new SqlCommand(query, myCon);
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                adapter.UpdateCommand = updateUser;
                adapter.UpdateCommand.ExecuteNonQuery();
                updateUser.Dispose();
                myCon.Close();
                return "Account updated successfully";
            }
            catch { myCon.Close(); return "Something went wrong. Please try again!"; }
        }


        ////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////   DONATIONS  //////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////

        [WebMethod]
        public string getDonationDate(string ID_donation)
        {
            DataSet dsDonations = new DataSet();
            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            string query = "SELECT * FROM Donations WHERE ID_donation='" + ID_donation + "'";
            SqlDataAdapter adapter = new SqlDataAdapter(query, myCon);
            adapter.Fill(dsDonations, "Donations");
            myCon.Close();
            try
            {
                DataRow dr = dsDonations.Tables["Donations"].Rows[0];
                DateTime date = new DateTime();
                date = DateTime.Parse(dr.ItemArray.GetValue(5).ToString());
                return date.ToString("d");
            }
            catch { myCon.Close(); return "Error loading data"; }//no donation found 
        }

        [WebMethod]
        public string addDonation(string ID_donor, string ID_center, string dateShortFormat)
        {   
            if (dateShortFormat == "") return "Please select date";

            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            myCon.Open();
            var rand = new Random(); //for donation id
            string ID_donation = rand.Next(10000, 99999).ToString();            
            string query = "INSERT INTO Donations (ID_donation, ID_doc, ID_donor, ID_center, blood_type, date, completed)" +
                            " VALUES ('" + ID_donation + "', 'default_doc', '" + ID_donor + "','" + ID_center + "', 'undetermied', '" + dateShortFormat + "', 0)";

            SqlCommand insertNewDonation = new SqlCommand(query, myCon);
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                adapter.UpdateCommand = insertNewDonation;
                adapter.UpdateCommand.ExecuteNonQuery();
                insertNewDonation.Dispose();
                myCon.Close();
                return "Donation scheduled successfully";
            }
            catch { myCon.Close(); return "Something went wrong. Please try again!"; }

        }

        [WebMethod]
        public string completeDonation(string ID_donation, string ID_doc, string blood_type)
        {          // ONLY TO BE USED BY DOCTOR

            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            myCon.Open();
            string query = "UPDATE Donations SET ID_doc = '" + ID_doc + "', blood_type = '" + blood_type + "', completed = 1" +
                                "WHERE ID_donation = '" + ID_donation + "'";

            SqlCommand updateDonation = new SqlCommand(query, myCon);
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {   
                adapter.UpdateCommand = updateDonation;
                adapter.UpdateCommand.ExecuteNonQuery();
                updateDonation.Dispose();
                myCon.Close();
                return "Donation completed successfully";
            }
            catch { myCon.Close(); return "Something went wrong. Please try again!"; }
        }

        [WebMethod]
        public List<string> getUserPastDonations(string ID_donor)
        {
            List<string> past_donations = new List<string>();

            DataSet dsDonations = new DataSet();
            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            myCon.Open();
            string query = "SELECT * FROM Donations WHERE ID_donor = '" + ID_donor + "' AND completed = 1 ORDER BY date DESC";
            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter(query, myCon);
                adapter.Fill(dsDonations, "Donations");
                myCon.Close();
                bool existent_past_donations = false;

                foreach (DataRow dr in dsDonations.Tables["Donations"].Rows)
                {
                    past_donations.Add("ID: " + dr.ItemArray.GetValue(0).ToString() +
                                       ", Doctor name: " + getDoctorFirstName(dr.ItemArray.GetValue(1).ToString()) + " "
                                          + getDoctorLastName(dr.ItemArray.GetValue(1).ToString()) +
                                       ", Date: " + getDonationDate(dr.ItemArray.GetValue(0).ToString()) +
                                       ", Center: " + getCenterCity((int)dr.ItemArray.GetValue(3)));
                    existent_past_donations = true;
                }

                if (!existent_past_donations) past_donations.Add("No donations made yet!");
                return past_donations;
            }
            catch 
            {
                myCon.Close();
                past_donations.Add("Error loading data");
                return past_donations;
            }
        }

        [WebMethod]
        public List<string> getUserFutureDonations(string ID_donor)
        {
            List<string> future_donations = new List<string>();

            DataSet dsDonations = new DataSet();
            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            myCon.Open();
            string query = "SELECT * FROM Donations WHERE ID_donor = '" + ID_donor + "' AND completed = 0 ORDER BY date DESC";
            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter(query, myCon);
                adapter.Fill(dsDonations, "Donations");
                myCon.Close();
                bool existent_future_donations = false;

                foreach (DataRow dr in dsDonations.Tables["Donations"].Rows)
                {
                    future_donations.Add("ID: " + dr.ItemArray.GetValue(0).ToString() +                                     
                                         ", Date: " + getDonationDate(dr.ItemArray.GetValue(0).ToString()) +
                                         ", Center: " + getCenterCity((int)dr.ItemArray.GetValue(3)));
                    existent_future_donations = true;
                }

                if (!existent_future_donations) future_donations.Add("No donations planned yet!");
                return future_donations;
            }
            catch
            {
                myCon.Close();
                future_donations.Add("Error loading data");
                return future_donations;
            }
        }

        [WebMethod]
        public List<string> getCenterPastDonations(string ID_center)
        {
            List<string> past_donations = new List<string>();

            DataSet dsDonations = new DataSet();
            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            myCon.Open();
            string query = "SELECT * FROM Donations WHERE ID_center = '" + ID_center + "' AND completed = 1 ORDER BY date DESC";
            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter(query, myCon);
                adapter.Fill(dsDonations, "Donations");
                myCon.Close();
                bool existent_past_donations = false;

                foreach (DataRow dr in dsDonations.Tables["Donations"].Rows)
                {
                    past_donations.Add("ID: " + dr.ItemArray.GetValue(0).ToString() +
                                       ", Donor name: " + getDonorFirstName(dr.ItemArray.GetValue(2).ToString()) + " "
                                          + getDonorLastName(dr.ItemArray.GetValue(2).ToString()) +
                                       ", Date: " + getDonationDate(dr.ItemArray.GetValue(0).ToString()) +
                                       ", Blood Type: " + dr.ItemArray.GetValue(4).ToString());
                    existent_past_donations = true;
                }

                if (!existent_past_donations) past_donations.Add("No donations made yet!");
                return past_donations;
            }
            catch
            {
                myCon.Close();
                past_donations.Add("Error loading data");
                return past_donations;
            }
        }

        [WebMethod]
        public List<string> getCenterFutureDonations(string ID_center)
        {
            List<string> future_donations = new List<string>();

            DataSet dsDonations = new DataSet();
            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            myCon.Open();
            string query = "SELECT * FROM Donations WHERE ID_center = '" + ID_center + "' AND completed = 0 ORDER BY date DESC";
            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter(query, myCon);
                adapter.Fill(dsDonations, "Donations");
                myCon.Close();
                bool existent_future_donations = false;

                foreach (DataRow dr in dsDonations.Tables["Donations"].Rows)
                {
                    future_donations.Add("ID: " + dr.ItemArray.GetValue(0).ToString() +
                                       ", Donor name: " + getDonorFirstName(dr.ItemArray.GetValue(2).ToString()) + " "
                                          + getDonorLastName(dr.ItemArray.GetValue(2).ToString()) +
                                       ", Date: " + getDonationDate(dr.ItemArray.GetValue(0).ToString()));
                    existent_future_donations = true;
                }

                if (!existent_future_donations) future_donations.Add("No donations planned yet!");
                return future_donations;
            }
            catch
            {
                myCon.Close();
                future_donations.Add("Error loading data");
                return future_donations;
            }
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////   CENTERS   ////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [WebMethod]
        public string getCenterCity(int ID_center)
        {
            DataSet dsCenters = new DataSet();
            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            string query = "SELECT * FROM Centers WHERE ID_center='" + ID_center + "'";
            SqlDataAdapter adapter = new SqlDataAdapter(query, myCon);
            adapter.Fill(dsCenters, "Centers");
            myCon.Close();
            try
            {
                DataRow dr = dsCenters.Tables["Centers"].Rows[0];
                string city = dr.ItemArray.GetValue(2).ToString();
                return city;
            }
            catch { myCon.Close(); return "Error loading data"; } //no center found
        }

        [WebMethod]
        public List<string> getCenterCountyList()
        {
            List<string> counties = new List<string>();
            DataSet dsCounties = new DataSet();
            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            string query = "SELECT * FROM Counties";
            SqlDataAdapter adapter = new SqlDataAdapter(query, myCon);
            adapter.Fill(dsCounties, "Counties");
            myCon.Close();
            try
            {
                foreach (DataRow dr in dsCounties.Tables["Counties"].Rows) 
                {
                    string county = dr.ItemArray.GetValue(0).ToString();
                    if(county!="-") counties.Add(county);
                }
                return counties;
            }
            catch 
            {
                myCon.Close();
                counties.Add("Error loading data");
                return counties; 
            } 
        }

        [WebMethod]
        public List<string> getCenterTownList(string county)
        {
            List<string> towns = new List<string>();
            DataSet dsCenters = new DataSet();
            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            string query = "SELECT * FROM Centers WHERE County = '" + county + "'";
            SqlDataAdapter adapter = new SqlDataAdapter(query, myCon);
            adapter.Fill(dsCenters, "Centers");
            myCon.Close();
            try
            {
                foreach (DataRow dr in dsCenters.Tables["Centers"].Rows)
                {
                    string town = dr.ItemArray.GetValue(2).ToString();
                    if(town!="-") towns.Add(town);
                }
                return towns;
            }
            catch
            {
                myCon.Close();
                towns.Add("Error loading data");
                return towns;
            }
        }

        [WebMethod]
        public string getCenterAddress(string town)
        {
            DataSet dsCenters = new DataSet();
            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            string query = "SELECT * FROM Centers WHERE Town = '" + town + "'";
            SqlDataAdapter adapter = new SqlDataAdapter(query, myCon);
            adapter.Fill(dsCenters, "Centers");
            myCon.Close();
            try
            {
                DataRow dr = dsCenters.Tables["Centers"].Rows[0];
                string address = dr.ItemArray.GetValue(3).ToString();
                return address;
            }
            catch { myCon.Close(); return "Error loading data"; }
        }

        [WebMethod]
        public string getCenterEmail(string town)
        {
            DataSet dsCenters = new DataSet();
            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            string query = "SELECT * FROM Centers WHERE Town = '" + town + "'";
            SqlDataAdapter adapter = new SqlDataAdapter(query, myCon);
            adapter.Fill(dsCenters, "Centers");
            myCon.Close();
            try
            {
                DataRow dr = dsCenters.Tables["Centers"].Rows[0];
                string email = dr.ItemArray.GetValue(4).ToString();
                return email;
            }
            catch { myCon.Close(); return "Error loading data"; }
        }

        [WebMethod]
        public string getCenterID(string town)
        {
            DataSet dsCenters = new DataSet();
            SqlConnection myCon = new SqlConnection();
            myCon.ConnectionString = getDBconnectionString();
            string query = "SELECT * FROM Centers WHERE Town = '" + town + "'";
            SqlDataAdapter adapter = new SqlDataAdapter(query, myCon);
            adapter.Fill(dsCenters, "Centers");
            myCon.Close();
            try
            {
                DataRow dr = dsCenters.Tables["Centers"].Rows[0];
                string email = dr.ItemArray.GetValue(0).ToString();
                return email;
            }
            catch { myCon.Close(); return "Error loading data"; }
        }

    }


}

