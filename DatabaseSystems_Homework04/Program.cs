using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using MySql.Data.MySqlClient;

namespace DatabaseSystems_Homework04
{
	internal class Program
	{
		// MySQL connection string
		private static string connectionString = "server=localhost;user=new_username1;database=hw04database;port=3306;password=new_password1";
		static void Main(string[] args)
		{
			EnsureDBTableExists();
			bool keepRunning = true;
			while (keepRunning)
			{
				Console.WriteLine("Choose an operation:");
				Console.WriteLine("1. Create a record");
				Console.WriteLine("2. Read records");
				Console.WriteLine("3. Update a record");
				Console.WriteLine("4. Delete a record");
				Console.WriteLine("5. Exit");
				var choice = Console.ReadLine();
				switch (choice)
				{
					case "1":
						CreateRecord();
						break;
					case "2":
						ReadRecords();
						break;
					case "3":
						UpdateRecord();
						break;
					case "4":
						DeleteRecord();
						break;
					case "5":
						keepRunning = false;
						break;
					default:
						Console.WriteLine("Invalid choice. Try again.");
						break;
				}
			}
		}

		static void CreateRecord()
		{
			string name;
			string email;
			string phone;
			string address;
			bool emailValid = false;
			bool phoneValid = false;

			// Input validation for "name"
			do
			{
				Console.Write("Enter Name: ");
				name = Console.ReadLine();
				if (string.IsNullOrEmpty(name))
				{
					Console.WriteLine("Name cannot be empty.");
				}
			}
			while (string.IsNullOrEmpty(name));
			// Input validation for Email
			do
			{
				Console.Write("Enter Email: ");
				email = Console.ReadLine();
				if (string.IsNullOrEmpty(email))
				{
					Console.WriteLine("Email cannot be empty.");
				}
				else if (!ValidateEmail(email))
				{
					Console.WriteLine("Email is invalid");
				}
				else
				{
					emailValid = true;
				}
			}
			while (string.IsNullOrEmpty(email) || emailValid == false);
			// Input validation for Phone Number
			do
			{
				Console.Write("Enter Phone Number: ");
				phone = Console.ReadLine();
				if (string.IsNullOrEmpty(phone))
				{
					Console.WriteLine("Phone Number cannot be empty.");
				}
				else if (!ValidatePhoneNumber(phone))
				{
					Console.WriteLine("Phone Number is invalid");
				}
				else
				{
					phoneValid = true;
				}
			}
			while (string.IsNullOrEmpty(phone) || phoneValid == false);
			// Input validation for Address
			do
			{
				Console.Write("Enter Address: ");
				address = Console.ReadLine();
				if (string.IsNullOrEmpty(address))
				{
					Console.WriteLine("Address cannot be empty.");
				}
			}

			while (string.IsNullOrEmpty(address));

			// Insert the record into MySQL
			using (MySqlConnection conn = new MySqlConnection(connectionString))
			{
				conn.Open();
				string query = "INSERT INTO USER (name, email, phone, address) VALUES (@name, @email, @phone, @address)";
				MySqlCommand cmd = new MySqlCommand(query, conn);
				cmd.Parameters.AddWithValue("@name", name);
				cmd.Parameters.AddWithValue("@email", email);
				cmd.Parameters.AddWithValue("@phone", phone);
				cmd.Parameters.AddWithValue("@address", address);
				cmd.ExecuteNonQuery();
				Console.WriteLine("Record inserted successfully.");
			}


		}

		static void ReadRecords()
		{
			using (MySqlConnection conn = new MySqlConnection(connectionString))
			{
				conn.Open();
				string query = "SELECT id, name, email, phone, address, created_at FROM USER";
				MySqlCommand cmd = new MySqlCommand(query, conn);
				MySqlDataReader reader = cmd.ExecuteReader();

				// Create a DataTable to display records
				DataTable dt = new DataTable();
				dt.Columns.Add("ID");
				dt.Columns.Add("Name");
				dt.Columns.Add("Email");
				dt.Columns.Add("Phone Number");
				dt.Columns.Add("Address");
				dt.Columns.Add("Created At");

				while (reader.Read())
				{
					DataRow row = dt.NewRow();
					row["ID"] = reader["id"].ToString();
					row["Name"] = reader["name"].ToString();
					row["Email"] = reader["email"].ToString();
					row["Phone Number"] = reader["phone"].ToString();
					row["Address"] = reader["address"].ToString();
					row["Created At"] = reader["created_at"].ToString();
					dt.Rows.Add(row);
				}
				PrintFormattedDataTable(dt);
			}
		}

		// Helper function to print DataTable in a nicely formatted way.
		static void PrintFormattedDataTable(DataTable dt)
		{
			// Find the maximum width of each cokumn
			int[] columnWidths = new int[dt.Columns.Count];

			for (int i = 0; i < dt.Columns.Count; i++)
			{
				columnWidths[i] = dt.Columns[i].ColumnName.Length;
				foreach (DataRow row in dt.Rows)
				{
					int length = row[i].ToString().Length;
					if (length > columnWidths[i])
					{
						columnWidths[i] = length;
					}
				}
			}

			// Print the headers with padding
			for (int i = 0; i < dt.Columns.Count; i++)
			{
				Console.Write(dt.Columns[i].ColumnName.PadRight(columnWidths[i] + 2));
			}
			Console.WriteLine();
			// Print the rows with padding
			foreach (DataRow row in dt.Rows)
			{
				for (int i = 0; i < dt.Columns.Count; i++)
				{
					Console.Write(row[i].ToString().PadRight(columnWidths[i] + 2));
				}
				Console.WriteLine();
			}

		}
		static void UpdateRecord()
		{
			Console.Write("Enter the ID of the person to update: ");
			string updateID = Console.ReadLine();

			using (MySqlConnection conn = new MySqlConnection(connectionString))
			{
				conn.Open();
				string query = "SELECT * FROM USER WHERE id = @id";
				MySqlCommand cmd = new MySqlCommand(query, conn);
				cmd.Parameters.AddWithValue("@id", updateID);
				MySqlDataReader reader = cmd.ExecuteReader();
				if (!reader.HasRows)
				{
					Console.WriteLine($"No record found with ID: {updateID}");
					return;
				}
				reader.Close();

				string newName;
				string newEmail;
				string newPhone;
				string newAddress;
				bool emailValid = false;
				bool phoneValid = false;

				// Input validation for "name"
				do
				{
					Console.Write("Enter new Name: ");
					newName = Console.ReadLine();
					if (string.IsNullOrEmpty(newName))
					{
						Console.WriteLine("Name cannot be empty.");
					}
				}
				while (string.IsNullOrEmpty(newName));
				// Input validation for Email
				do
				{
					Console.Write("Enter new email: ");
					newEmail = Console.ReadLine();
					if (string.IsNullOrEmpty(newEmail))
					{
						Console.WriteLine("Email cannot be empty.");
					}
					else if (!ValidateEmail(newEmail))
					{
						Console.WriteLine("Email is invalid");
					}
					else
					{
						emailValid = true;
					}
				}
				while (string.IsNullOrEmpty(newEmail) || emailValid == false);
				// Input validation for Phone Number
				do
				{
					Console.Write("Enter new Phone Number: ");
					newPhone = Console.ReadLine();
					if (string.IsNullOrEmpty(newPhone))
					{
						Console.WriteLine("Phone Number cannot be empty.");
					}
					else if (!ValidatePhoneNumber(newPhone))
					{
						Console.WriteLine("Phone Number is invalid");
					}
					else
					{
						phoneValid = true;
					}
				}
				while (string.IsNullOrEmpty(newPhone) || phoneValid == false);
				// Input validation for Address
				do
				{
					Console.Write("Enter new Address: ");
					newAddress = Console.ReadLine();
					if (string.IsNullOrEmpty(newAddress))
					{
						Console.WriteLine("Address cannot be empty.");
					}
				}
				while (string.IsNullOrEmpty(newAddress));

				string updateQuery = "UPDATE USER SET name = @newName, email = @newEmail, phone = @newPhone, address = @newAddress WHERE id = @pid";
				MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn);
				updateCmd.Parameters.AddWithValue("@newName", newName);
				updateCmd.Parameters.AddWithValue("@newEmail", newEmail);
				updateCmd.Parameters.AddWithValue("@newPhone", newPhone);
				updateCmd.Parameters.AddWithValue("@newAddress", newAddress);
				updateCmd.Parameters.AddWithValue("@pid", updateID);
				int result = updateCmd.ExecuteNonQuery();
				if (result > 0)
				{
					Console.WriteLine("Record updated successfully");
				}
				else
				{
					Console.WriteLine("No records updated.");
				}

			}
		}

		static void DeleteRecord()
		{
			Console.Write("Enter the ID of the person to delete: ");
			string delID = Console.ReadLine();

			using (MySqlConnection conn = new MySqlConnection(connectionString))
			{
				conn.Open();
				string delQuery = "DELETE FROM USER WHERE id = @id";
				MySqlCommand cmd = new MySqlCommand(delQuery, conn);
				cmd.Parameters.AddWithValue("@id", delID);
				int result = cmd.ExecuteNonQuery();
				if (result > 0)
				{
					Console.WriteLine("Record deleted successfully");
				}
				else
				{
					Console.WriteLine("No records deleted.");
				}

			}
		}

		static void EnsureDBTableExists()
		{
			using (MySqlConnection conn = new MySqlConnection(connectionString))
			{
				conn.Open();
				string createTableQuery = @"
				CREATE TABLE IF NOT EXISTS `USER` (
				`id` INT AUTO_INCREMENT PRIMARY KEY,
				`name` VARCHAR(100) NOT NULL,
				`email` VARCHAR(255) NOT NULL,
				`phone` VARCHAR(100) NOT NULL,
				`address` VARCHAR(255) NOT NULL,
				`created_at` DATETIME DEFAULT CURRENT_TIMESTAMP
				);";
				MySqlCommand cmd = new MySqlCommand(createTableQuery, conn);
				cmd.ExecuteNonQuery();
				Console.WriteLine("Table 'USER' is ensured to exist");
			}
		}
		static bool ValidatePhoneNumber(string phoneNumber)
		{
			Regex regex = new Regex(@"^\d{10,}$");
			Match match = regex.Match(phoneNumber);
			if (match.Success)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		static bool ValidateEmail(string email)
		{
			Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,})+)$");
			Match match = regex.Match(email);
			if (match.Success)
			{
				return true;
			}
			else
			{
				return false;
			}
		}


	}
}
