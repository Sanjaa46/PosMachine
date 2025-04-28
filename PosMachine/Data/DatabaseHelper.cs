using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using PosMachine.Models;
using POSMachine.Models;

namespace POSMachine.Data
{
    public class DatabaseHelper
    {
        private static string _connectionString = "Data Source=posdb.sqlite;Version=3;";

        public static void InitializeDatabase()
        {
            // Create database if it doesn't exist
            if (!File.Exists("posdb.sqlite"))
            {
                SQLiteConnection.CreateFile("posdb.sqlite");

                // Create tables
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    // Create Users table
                    string createUserTableSql = @"
                        CREATE TABLE Users (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Username TEXT NOT NULL UNIQUE,
                            Password TEXT NOT NULL,
                            Role INTEGER NOT NULL
                        );";

                    // Create Categories table
                    string createCategoryTableSql = @"
                        CREATE TABLE Categories (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL UNIQUE
                        );";

                    // Create Products table
                    string createProductTableSql = @"
                        CREATE TABLE Products (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Code TEXT NOT NULL UNIQUE,
                            Name TEXT NOT NULL,
                            Price REAL NOT NULL,
                            CategoryId INTEGER,
                            Image BLOB,
                            FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
                        );";

                    // Create Orders table
                    string createOrderTableSql = @"
                        CREATE TABLE Orders (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            OrderDate TEXT NOT NULL,
                            UserId INTEGER NOT NULL,
                            Subtotal REAL NOT NULL,
                            CGST REAL NOT NULL,
                            IGST REAL NOT NULL,
                            Total REAL NOT NULL,
                            AmountPaid REAL NOT NULL,
                            Change REAL NOT NULL,
                            FOREIGN KEY (UserId) REFERENCES Users(Id)
                        );";

                    // Create OrderItems table
                    string createOrderItemTableSql = @"
                        CREATE TABLE OrderItems (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            OrderId INTEGER NOT NULL,
                            ProductId INTEGER NOT NULL,
                            ProductName TEXT NOT NULL,
                            Price REAL NOT NULL,
                            Quantity INTEGER NOT NULL,
                            FOREIGN KEY (OrderId) REFERENCES Orders(Id),
                            FOREIGN KEY (ProductId) REFERENCES Products(Id)
                        );";

                    using (var command = new SQLiteCommand(connection))
                    {
                        command.CommandText = createUserTableSql;
                        command.ExecuteNonQuery();

                        command.CommandText = createCategoryTableSql;
                        command.ExecuteNonQuery();

                        command.CommandText = createProductTableSql;
                        command.ExecuteNonQuery();

                        command.CommandText = createOrderTableSql;
                        command.ExecuteNonQuery();

                        command.CommandText = createOrderItemTableSql;
                        command.ExecuteNonQuery();
                    }

                    // Insert default users
                    string insertUsersSql = @"
                        INSERT INTO Users (Username, Password, Role) VALUES 
                        ('Manager', 'password', 0),
                        ('Cashier1', 'password', 1),
                        ('Cashier2', 'password', 1);";

                    // Insert sample categories
                    string insertCategoriesSql = @"
                        INSERT INTO Categories (Name) VALUES 
                        ('Pizza'),
                        ('Pasta'),
                        ('Sandwich');";

                    // Insert sample products
                    string insertProductsSql = @"
                        INSERT INTO Products (Code, Name, Price, CategoryId) VALUES 
                        ('1', 'Margherita', 100.0, 1),
                        ('2', 'Marinara', 200.0, 1),
                        ('3', 'Vegetarians', 150.0, 1),
                        ('4', 'Alfedo', 200.0, 1),
                        ('5', 'Spaghetti Pasta', 150.0, 2),
                        ('6', 'White Sauce Pasta', 200.0, 2),
                        ('8', 'American Sub', 100.0, 3);";

                    using (var command = new SQLiteCommand(connection))
                    {
                        command.CommandText = insertUsersSql;
                        command.ExecuteNonQuery();

                        command.CommandText = insertCategoriesSql;
                        command.ExecuteNonQuery();

                        command.CommandText = insertProductsSql;
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        // User methods
        public static User AuthenticateUser(string username, string password)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM Users WHERE Username = @Username AND Password = @Password;";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Username = reader["Username"].ToString(),
                                Password = reader["Password"].ToString(),
                                Role = (UserRole)Convert.ToInt32(reader["Role"])
                            };
                        }
                    }
                }
            }

            return null;
        }

        // Product methods
        public static List<Product> GetAllProducts()
        {
            var products = new List<Product>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM Products;";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var product = new Product
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Code = reader["Code"].ToString(),
                                Name = reader["Name"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                CategoryId = Convert.ToInt32(reader["CategoryId"])
                            };

                            if (reader["Image"] != DBNull.Value)
                            {
                                product.Image = (byte[])reader["Image"];
                            }

                            products.Add(product);
                        }
                    }
                }
            }

            return products;
        }

        public static Product GetProductByCode(string code)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM Products WHERE Code = @Code;";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Code", code);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var product = new Product
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Code = reader["Code"].ToString(),
                                Name = reader["Name"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                CategoryId = Convert.ToInt32(reader["CategoryId"])
                            };

                            if (reader["Image"] != DBNull.Value)
                            {
                                product.Image = (byte[])reader["Image"];
                            }

                            return product;
                        }
                    }
                }
            }

            return null;
        }

        public static void SaveProduct(Product product)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                if (product.Id == 0)
                {
                    // Insert new product
                    string sql = @"
                        INSERT INTO Products (Code, Name, Price, CategoryId, Image) 
                        VALUES (@Code, @Name, @Price, @CategoryId, @Image);
                        SELECT last_insert_rowid();";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Code", product.Code);
                        command.Parameters.AddWithValue("@Name", product.Name);
                        command.Parameters.AddWithValue("@Price", product.Price);
                        command.Parameters.AddWithValue("@CategoryId", product.CategoryId);

                        if (product.Image != null)
                        {
                            command.Parameters.AddWithValue("@Image", product.Image);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@Image", DBNull.Value);
                        }

                        product.Id = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
                else
                {
                    // Update existing product
                    string sql = @"
                        UPDATE Products 
                        SET Code = @Code, Name = @Name, Price = @Price, CategoryId = @CategoryId, Image = @Image
                        WHERE Id = @Id;";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", product.Id);
                        command.Parameters.AddWithValue("@Code", product.Code);
                        command.Parameters.AddWithValue("@Name", product.Name);
                        command.Parameters.AddWithValue("@Price", product.Price);
                        command.Parameters.AddWithValue("@CategoryId", product.CategoryId);

                        if (product.Image != null)
                        {
                            command.Parameters.AddWithValue("@Image", product.Image);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@Image", DBNull.Value);
                        }

                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public static void DeleteProduct(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = "DELETE FROM Products WHERE Id = @Id;";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Category methods
        public static List<Category> GetAllCategories()
        {
            var categories = new List<Category>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM Categories;";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(new Category
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString()
                            });
                        }
                    }
                }
            }

            return categories;
        }

        // Order methods
        public static int SaveOrder(Order order)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                // Begin transaction
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insert order
                        string orderSql = @"
                            INSERT INTO Orders (OrderDate, UserId, Subtotal, CGST, IGST, Total, AmountPaid, Change)
                            VALUES (@OrderDate, @UserId, @Subtotal, @CGST, @IGST, @Total, @AmountPaid, @Change);
                            SELECT last_insert_rowid();";

                        using (var command = new SQLiteCommand(orderSql, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@OrderDate", order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"));
                            command.Parameters.AddWithValue("@UserId", order.UserId);
                            command.Parameters.AddWithValue("@Subtotal", order.Subtotal);
                            command.Parameters.AddWithValue("@CGST", order.CGST);
                            command.Parameters.AddWithValue("@IGST", order.IGST);
                            command.Parameters.AddWithValue("@Total", order.Total);
                            command.Parameters.AddWithValue("@AmountPaid", order.AmountPaid);
                            command.Parameters.AddWithValue("@Change", order.Change);

                            order.Id = Convert.ToInt32(command.ExecuteScalar());
                        }

                        // Insert order items
                        string itemSql = @"
                            INSERT INTO OrderItems (OrderId, ProductId, ProductName, Price, Quantity)
                            VALUES (@OrderId, @ProductId, @ProductName, @Price, @Quantity);";

                        foreach (var item in order.Items)
                        {
                            using (var command = new SQLiteCommand(itemSql, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@OrderId", order.Id);
                                command.Parameters.AddWithValue("@ProductId", item.ProductId);
                                command.Parameters.AddWithValue("@ProductName", item.ProductName);
                                command.Parameters.AddWithValue("@Price", item.Price);
                                command.Parameters.AddWithValue("@Quantity", item.Quantity);

                                command.ExecuteNonQuery();
                            }
                        }

                        // Commit transaction
                        transaction.Commit();
                        return order.Id;
                    }
                    catch
                    {
                        // Rollback transaction if an error occurs
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}