using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using POSMachine.Models;

namespace POSMachine.Data
{
    public class DatabaseHelper
    {
        private static readonly string _dbPath = "posdb.sqlite";
        private static readonly string _connectionString = $"Data Source={_dbPath};Version=3;";

        public static void InitializeDatabase()
        {
            try
            {
                // Create database if it doesn't exist
                if (!File.Exists(_dbPath))
                {
                    SQLiteConnection.CreateFile(_dbPath);
                    CreateDatabaseSchema();
                    PopulateInitialData();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to initialize database: {ex.Message}", ex);
            }
        }

        private static void CreateDatabaseSchema()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
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

                        ExecuteNonQuery(connection, transaction, createUserTableSql);
                        ExecuteNonQuery(connection, transaction, createCategoryTableSql);
                        ExecuteNonQuery(connection, transaction, createProductTableSql);
                        ExecuteNonQuery(connection, transaction, createOrderTableSql);
                        ExecuteNonQuery(connection, transaction, createOrderItemTableSql);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static void PopulateInitialData()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
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

                        ExecuteNonQuery(connection, transaction, insertUsersSql);
                        ExecuteNonQuery(connection, transaction, insertCategoriesSql);
                        ExecuteNonQuery(connection, transaction, insertProductsSql);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static void ExecuteNonQuery(SQLiteConnection connection, SQLiteTransaction transaction, string sql)
        {
            using (var command = new SQLiteCommand(sql, connection, transaction))
            {
                command.ExecuteNonQuery();
            }
        }

        // Improved AuthenticateUser method
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

        // Additional Database Helper methods with improved connection management

        // Get all products
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

        // Get product by code
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

        // Save product (insert or update)
        public static void SaveProduct(Product product)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        if (product.Id == 0)
                        {
                            // Insert new product
                            string sql = @"
                        INSERT INTO Products (Code, Name, Price, CategoryId, Image) 
                        VALUES (@Code, @Name, @Price, @CategoryId, @Image);
                        SELECT last_insert_rowid();";

                            using (var command = new SQLiteCommand(sql, connection, transaction))
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

                            using (var command = new SQLiteCommand(sql, connection, transaction))
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

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Delete product
        public static void DeleteProduct(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string sql = "DELETE FROM Products WHERE Id = @Id;";

                        using (var command = new SQLiteCommand(sql, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Id", id);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Get all categories
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

        // Save category (insert or update)
        public static void SaveCategory(Category category)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        if (category.Id == 0)
                        {
                            // Insert new category
                            string sql = "INSERT INTO Categories (Name) VALUES (@Name); SELECT last_insert_rowid();";

                            using (var command = new SQLiteCommand(sql, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@Name", category.Name);
                                category.Id = Convert.ToInt32(command.ExecuteScalar());
                            }
                        }
                        else
                        {
                            // Update existing category
                            string sql = "UPDATE Categories SET Name = @Name WHERE Id = @Id;";

                            using (var command = new SQLiteCommand(sql, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@Name", category.Name);
                                command.Parameters.AddWithValue("@Id", category.Id);
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Delete category
        public static void DeleteCategory(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // First check if the category is in use
                        string checkSql = "SELECT COUNT(*) FROM Products WHERE CategoryId = @CategoryId;";
                        bool categoryInUse;

                        using (var command = new SQLiteCommand(checkSql, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@CategoryId", id);
                            int count = Convert.ToInt32(command.ExecuteScalar());
                            categoryInUse = count > 0;
                        }

                        if (categoryInUse)
                        {
                            throw new InvalidOperationException("This category cannot be deleted because it is used by one or more products.");
                        }

                        // Delete category if not in use
                        string deleteSql = "DELETE FROM Categories WHERE Id = @Id;";

                        using (var command = new SQLiteCommand(deleteSql, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Id", id);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Save order with items
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
                        // Insert order - NO TAX FIELDS
                        string orderSql = @"
                INSERT INTO Orders (OrderDate, UserId, Subtotal, Total, AmountPaid, Change)
                VALUES (@OrderDate, @UserId, @Subtotal, @Total, @AmountPaid, @Change);
                SELECT last_insert_rowid();";

                        using (var command = new SQLiteCommand(orderSql, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@OrderDate", order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"));
                            command.Parameters.AddWithValue("@UserId", order.UserId);
                            command.Parameters.AddWithValue("@Subtotal", order.Subtotal);
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

        // Get order by ID (with items)
        public static Order GetOrderById(int orderId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                // First get the order
                string orderSql = "SELECT * FROM Orders WHERE Id = @OrderId;";
                Order order = null;

                using (var command = new SQLiteCommand(orderSql, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            order = new Order
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                OrderDate = DateTime.Parse(reader["OrderDate"].ToString()),
                                UserId = Convert.ToInt32(reader["UserId"]),
                                Subtotal = Convert.ToDecimal(reader["Subtotal"]),
                                Total = Convert.ToDecimal(reader["Total"]),
                                AmountPaid = Convert.ToDecimal(reader["AmountPaid"]),
                                Change = Convert.ToDecimal(reader["Change"]),
                                Items = new List<OrderItem>()
                            };
                        }
                    }
                }

                // Get order items
                if (order != null)
                {
                    string itemsSql = "SELECT * FROM OrderItems WHERE OrderId = @OrderId;";

                    using (var command = new SQLiteCommand(itemsSql, connection))
                    {
                        command.Parameters.AddWithValue("@OrderId", orderId);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new OrderItem
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    OrderId = orderId,
                                    ProductId = Convert.ToInt32(reader["ProductId"]),
                                    ProductName = reader["ProductName"].ToString(),
                                    Price = Convert.ToDecimal(reader["Price"]),
                                    Quantity = Convert.ToInt32(reader["Quantity"])
                                };

                                order.Items.Add(item);
                            }
                        }
                    }
                }

                return order;
            }
        }

        // Get user by ID
        public static User GetUserById(int userId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM Users WHERE Id = @UserId;";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

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

        // Get recent orders
        public static List<Order> GetRecentOrders(int count = 10)
        {
            var orders = new List<Order>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = @"
            SELECT * FROM Orders 
            ORDER BY OrderDate DESC 
            LIMIT @Count;";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Count", count);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new Order
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                OrderDate = DateTime.Parse(reader["OrderDate"].ToString()),
                                UserId = Convert.ToInt32(reader["UserId"]),
                                Subtotal = Convert.ToDecimal(reader["Subtotal"]),
                                Total = Convert.ToDecimal(reader["Total"]),
                                AmountPaid = Convert.ToDecimal(reader["AmountPaid"]),
                                Change = Convert.ToDecimal(reader["Change"])
                            });
                        }
                    }
                }
            }

            return orders;
        }
    }
}