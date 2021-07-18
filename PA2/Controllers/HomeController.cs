using PA2.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Security.Cryptography;
using System;

namespace PA2.Controllers
{
    public class HomeController : Controller
    {
        DataContext Db = new DataContext();
        MD5 md5_thing = new MD5CryptoServiceProvider();

        public string hashMd5(string password)
        {
            byte[] hash_bytes = md5_thing.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder hashed_md5 = new StringBuilder(hash_bytes.Length * 2);

            for (int i = 0; i < hash_bytes.Length; i++)
            {
                hashed_md5.Append(hash_bytes[i].ToString("x2"));
            }

            return hashed_md5.ToString();
        }


        public ActionResult Index()
        {
            if (Session["CustomerID"] != null)
            {
                return RedirectToAction("Orders");
            }
            CheckMessages();
            return View();
        }

        // Login section
        [HttpPost]
        public ActionResult Index(Customer customer)
        {
            try
            {
                List<object> login = new List<object>
                {
                    customer.CustomerUsername,
                    hashMd5(customer.CustomerPassword)
                };

                object[] loginThings = login.ToArray();
                var data = Db.Customers.SqlQuery("SELECT * FROM Customers WHERE CustomerUsername=@p0 AND CustomerPassword=@p1", loginThings).SingleOrDefault();

                //Login Success
                if (data != null)
                {
                    Session["CustomerID"] = data.CustomerID;
                    Session["CustomerUsername"] = data.CustomerUsername;
                    Session["CustomerAdmin"] = data.CustomerAdmin;
                    return RedirectToAction("Orders");
                }
                else
                {
                    ViewBag.msg = "Username or password is incorrect. Try again";
                    return View();
                }
            } catch (Exception ex)
            {
                ViewBag.msg = "Something went wrong. Please try again (" + ex.Message + ")";
                return View();
            }
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(Customer customer)
        {
            try
            {
                List<object> register = new List<object>
                {
                    customer.CustomerUsername,
                    hashMd5(customer.CustomerPassword)
                };

                object[] registerThings = register.ToArray();
                int result = Db.Database.ExecuteSqlCommand("INSERT INTO Customers (CustomerUsername, CustomerPassword, CustomerAdmin) VALUES (@p0, @p1, 0)", registerThings);

                if (result > 0)
                {
                    return RedirectWithMessage("successMessage", "Registration successful! Thank you for signing up with ABC Food Catering.", "Index");
                } else
                {
                    ViewBag.msg = "Error while registering, please try again.";
                }
                return View();
            } catch (Exception ex)
            {
                ViewBag.msg = "Something went wrong, please try again. (" + ex.Message + ")";
                return View();
            }
        }

        public ActionResult Orders()
        {
            object data;
            if (Session["CustomerID"] != null)
            {
                if (Session["CustomerAdmin"] as string == "1")
                {
                    data = Db.Orders.SqlQuery("SELECT * FROM Orders");
                }
                else
                {
                    data = Db.Orders.SqlQuery("SELECT * FROM Orders WHERE CustomerID = " + Session["CustomerID"]);
                }
                CheckMessages();
                return View(data);
            } else
            {
                return RedirectWithMessage("errorMessage", "Please log in to view this page", "Index");
            }
        }

        public ActionResult Create()
        {
            if (Session["CustomerID"] != null)
            {
                return View();
            }
            else
            {
                return RedirectWithMessage("errorMessage", "Please log in to view this page", "Index");
            }
        }

        [HttpPost]
        public ActionResult Create(Orders orders)
        {
            if (Session["CustomerID"] != null)
            {
                try
                {
                    List<object> order = new List<object>
                    {
                        Session["CustomerID"],
                        orders.OrderDescription,
                        "Processing",
                        orders.DeliveryAddress,
                        orders.DeliveryDate,
                        orders.DeliveryTime,
                        orders.DeliveryContact
                    };

                    object[] orderThings = order.ToArray();
                    int result = Db.Database.ExecuteSqlCommand("INSERT INTO Orders (CustomerID, OrderDescription, OrderStatus, DeliveryAddress, DeliveryDate, DeliveryTime, DeliveryContact) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6)", orderThings);

                    if (result > 0)
                    {
                        return RedirectWithMessage("successMessage", "Order has been placed successfully, it will be processed shortly by our staff.", "Index");
                    }
                    else
                    {
                        ViewBag.msg = "Order unsuccessful. Please try again";
                        return View();
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.msg = "Something went wrong, please try again. (" + ex.Message + ")";
                    return View();
                }
            } else
            {
                return RedirectWithMessage("errorMessage", "Please log in to view this page", "Index");
            }
        }

        public ActionResult Details(int id)
        {
            if (Session["CustomerID"] != null)
            {
                try
                {
                    var order = Db.Orders.SqlQuery("SELECT * FROM Orders WHERE OrderID=" + id).SingleOrDefault();
                    if (order.CustomerID.ToString() == Session["CustomerID"].ToString() || Session["CustomerAdmin"] as string == "1")
                    {
                        if (order != null)
                        {
                            return View(order);
                        }
                        else
                        {
                            return RedirectWithMessage("errorMessage", "Unable to get details, order does not exist", "Orders");
                        }
                    } else
                    {
                        return RedirectWithMessage("errorMessage", "Insufficent permissions to view order", "Orders");
                    }
                }
                catch (Exception ex)
                {
                    return RedirectWithMessage("errorMessage", "Something went wrong, please try again. (" + ex.Message + ")", "Orders");
                }
            } else
            {
                return RedirectWithMessage("errorMessage", "Please log in to view this page", "Index");
            }
            
        }


        // Edit GET order and display it
        public ActionResult Edit(int id)
        {
            if (Session["CustomerID"] != null)
            {
                try
                {
                    var order = Db.Orders.SqlQuery("SELECT * FROM Orders WHERE OrderID=" + id).SingleOrDefault();
                    if (order != null)
                    {
                        if (order.OrderStatus == "Processing" && order.CustomerID.ToString() == Session["CustomerID"].ToString() || Session["CustomerAdmin"] as string == "1")
                        {
                            return View(order);
                        }
                        else
                        {
                            return RedirectWithMessage("errorMessage", "Insufficent permissions to edit order", "Orders");
                        }
                    }
                    else
                    {
                        return RedirectWithMessage("errorMessage", "Unable to edit, order does not exist", "Orders");
                    }
                }
                catch (Exception ex)
                {
                    return RedirectWithMessage("errorMessage", "Something went wrong, please try again. (" + ex.Message + ")", "Orders");
                }
            } else
            {
                return RedirectWithMessage("errorMessage", "Please log in to view this page", "Index");
            }
        }

        // Edit POST order into database
        [HttpPost]
        public ActionResult Edit(Orders orders)
        {
            if (Session["CustomerID"] != null)
            {
                try
                {
                    if (orders.OrderStatus == "Processing" && orders.CustomerID.ToString() == Session["CustomerID"].ToString() || Session["CustomerAdmin"] as string == "1") {
                        List<object> order = new List<object>
                        {
                            orders.OrderDescription,
                            orders.OrderStatus,
                            orders.DeliveryAddress,
                            orders.DeliveryDate,
                            orders.DeliveryTime,
                            orders.DeliveryContact,
                            orders.OrderID
                        };

                        object[] orderThings = order.ToArray();
                        int result = Db.Database.ExecuteSqlCommand("UPDATE Orders SET OrderDescription=@p0, OrderStatus=@p1, DeliveryAddress=@p2, DeliveryDate=@p3, DeliveryTime=@p4, DeliveryContact=@p5 WHERE OrderID=@p6", orderThings);

                        if (result > 0)
                        {
                            ViewBag.successMsg = "Successfully edited order!";
                            return View();
                        }
                        else
                        {
                            ViewBag.msg = "Order edit unsuccessful. Please try again";
                            return View();
                        }
                    } else
                    {
                        return RedirectWithMessage("errorMessage", "Insufficent permissions to edit order" + orders.CustomerID, "Orders");
                    }
                }
                catch (Exception ex)
                {
                    return RedirectWithMessage("errorMessage", "Something went wrong, please try again. (" + ex.Message + ")", "Orders");
                }
            } else
            {
                return RedirectWithMessage("errorMessage", "Please log in to view this page", "Index");
            }
            
        }

        // Delete section
        public ActionResult Delete(int id)
        {
            if (Session["CustomerID"] != null)
            {
                try
                {
                    var order = Db.Orders.SqlQuery("SELECT * FROM Orders WHERE OrderID=" + id).SingleOrDefault();
                    if (order != null)
                    {
                        // Checks whether order status is still processing AND whether the user is the one that created the account OR whether user is a admin which can modify any order
                        if (order.OrderStatus == "Processing" && order.CustomerID.ToString() == Session["CustomerID"].ToString() || Session["CustomerAdmin"] as string == "1")
                        {
                            return View(order);
                        } else
                        {
                            return RedirectWithMessage("errorMessage", "Insufficent permissions to delete order", "Orders");
                        }
                    }
                    else
                    {
                        return RedirectWithMessage("errorMessage", "Unable to delete, order does not exist", "Orders");
                    }
                }
                catch (Exception ex)
                {
                    return RedirectWithMessage("errorMessage", "Something went wrong, please try again. (" + ex.Message + ")", "Orders");
                }
            } else
            {
                return RedirectWithMessage("errorMessage", "Please log in to view this page", "Index");
            }
        }

        [HttpPost]
        public ActionResult Delete(Orders order, int id)
        {
            // TEMP
            try
            {
                if (order.CustomerID.ToString() == Session["CustomerID"].ToString() || Session["CustomerAdmin"] as string == "1")
                {
                    int result = Db.Database.ExecuteSqlCommand("DELETE FROM Orders WHERE OrderID = " + id);
                    if (result > 0)
                    {
                        return RedirectWithMessage("successMessage", "Successfully deleted order", "Orders");
                    } else
                    {
                        ViewBag.msg = "Unable to delete order";
                        return View(order);
                    }
                } else
                {
                    return RedirectWithMessage("errorMessage", "Insufficent permissions to delete order", "Orders");
                }
            } catch (Exception ex)
            {
                return RedirectWithMessage("errorMessage", "Something went wrong, please try again. (" + ex.Message + ")", "Orders");
            }
        }

        public ActionResult Users()
        {
            if (Session["CustomerID"] != null && Session["CustomerAdmin"] as string == "1")
            {
                try
                {
                    var data = Db.Customers.SqlQuery("SELECT *  FROM Customers");
                    if (data != null)
                    {
                        CheckMessages();
                        return View(data);
                    }
                    else
                    {
                        ViewBag.msg = "Unable to get user list";
                        return View();
                    }
                } catch (Exception ex)
                {
                    return RedirectWithMessage("errorMessage", "Something went wrong, please try again. (" + ex.Message + ")", "Orders");
                }
            } else
            {
                return RedirectWithMessage("errorMessage", "Insufficent permissions to view users", "Orders");
            }
        }

        public ActionResult CreateUser()
        {
            if (Session["CustomerID"] != null && Session["CustomerAdmin"] as string == "1")
            {
                return View();
            } else
            {
                return RedirectWithMessage("errorMessage", "Insufficent permissions to create user", "Orders");
            }
        }

        [HttpPost]
        public ActionResult CreateUser(Customer customer)
        {
            if (Session["CustomerID"] != null && Session["CustomerAdmin"] as string == "1")
            {
                try
                {
                    List<object> NewCustomer = new List<object>
                    {
                        customer.CustomerUsername,
                        hashMd5(customer.CustomerPassword),
                        customer.CustomerAdmin
                    };
                    object[] customerThings = NewCustomer.ToArray();
                    int result = Db.Database.ExecuteSqlCommand("INSERT INTO Customers (CustomerUsername, CustomerPassword, CustomerAdmin) VALUES (@p0, @p1, @p2)", customerThings);
                    if (result > 0)
                    {
                        return RedirectWithMessage("successMessage", "User has been created successfully", "Users");
                    } else
                    {
                        ViewBag.msg = "User creation not successful, please try again";
                        return View();
                    }
                } catch (Exception ex)
                {
                    ViewBag.msg = "Something went wrong, please try again. (" + ex.Message + ")";
                    return View();
                }
                
            } else
            {
                return RedirectWithMessage("errorMessage", "Insufficent permissions to create user", "Orders");
            }
        }

        public ActionResult DeleteUser(int id)
        {
            if (Session["CustomerID"] != null && Session["CustomerAdmin"] as string == "1")
            {
                try
                {
                    var data = Db.Customers.SqlQuery("SELECT * FROM Customers WHERE CustomerID = " + id).SingleOrDefault();

                    if (data != null)
                    {
                        return View(data);
                    } else
                    {
                        return RedirectWithMessage("errorMessage", "Unable to delete, customer does not exist", "Users");
                    }
                } catch (Exception ex)
                {
                    return RedirectWithMessage("errorMessage", "Something went wrong, please try again. (" + ex.Message + ")", "Users");
                }
            } else
            {
                return RedirectWithMessage("errorMessage", "Insufficent permissions to delete user", "Orders");
            }
        }

        [HttpPost]
        public ActionResult DeleteUser(Customer customer)
        {
            if (Session["CustomerID"] != null && Session["CustomerAdmin"] as string == "1")
            {
                try
                {
                    int result = Db.Database.ExecuteSqlCommand("DELETE FROM Customers WHERE CustomerID = " + customer.CustomerID);

                    if (result > 0)
                    {
                        return RedirectWithMessage("successMessage", "customer has been deleted successfully", "Users");
                    } else
                    {
                        return RedirectWithMessage("errorMessage", "Unable to delete customer, please try again", "Users");
                    }
                } catch (Exception ex)
                {
                    return RedirectWithMessage("errorMessage", "Something went wrong, please try again. (" + ex.Message + ")", "Users");
                }
            } else
            {
                return RedirectWithMessage("errorMessage", "Insufficent permissions to delete user", "Orders");
            }
        }

        public ActionResult EditUser(int id)
        {
            if (Session["CustomerID"] != null && Session["CustomerAdmin"] as string == "1")
            {
                try
                {
                    var data = Db.Customers.SqlQuery("SELECT * FROM Customers WHERE CustomerID = " + id).SingleOrDefault();
                    if (data != null)
                    {
                        return View(data);
                    } else
                    {
                        return RedirectWithMessage("errorMessage", "Unable to edit, customer ", "Users");
                    }
                } catch (Exception ex)
                {
                    return RedirectWithMessage("errorMessage", "Something went wrong, please try again. (" + ex.Message + ")", "Users");
                }
            } else
            {
                return RedirectWithMessage("errorMessage", "Insufficent permissions to edit user", "Orders");
            }
        }

        [HttpPost]
        public ActionResult EditUser(Customer customer)
        {
            if (Session["CustomerID"] != null && Session["CustomerAdmin"] as string == "1")
            {
                try
                {
                    List<object> NewCustomer = new List<object>
                    {
                        customer.CustomerID,
                        customer.CustomerUsername,
                        hashMd5(customer.CustomerPassword),
                        customer.CustomerAdmin
                    };
                    object[] customerThings = NewCustomer.ToArray();
                    int result = 0;
                    if (customer.CustomerPassword == "")
                    {
                        result = Db.Database.ExecuteSqlCommand("UPDATE Customers SET CustomerUsername = @p1, CustomerAdmin= @p3 WHERE CustomerID = @p0", customerThings);
                    } else
                    {
                        result = Db.Database.ExecuteSqlCommand("UPDATE Customers SET CustomerUsername = @p1, CustomerPassword = @p2, CustomerAdmin= @p3 WHERE CustomerID = @p0", customerThings);
                    }
                    if (result > 0 )
                    {
                        ViewBag.successMsg = "Customer data has been edited successfully.";
                        return View();
                    } else
                    {
                        ViewBag.successMsg = "Customer data has not been edited successfully, please try again.";
                        return View();
                    }
                } catch (Exception ex)
                {
                    return RedirectWithMessage("errorMessage", "Something went wrong, please try again. (" + ex.Message + ")", "Users");
                }
            } else
            {
                return RedirectWithMessage("errorMessage", "Insufficent permissions to edit user", "Orders");
            }
        }

        // Logout section
        public ActionResult Logout()
        {
            // Logout, clear session and redirect to login with successful logout message
            Session["CustomerID"] = null;
            Session["CustomerUsername"] = null;
            Session["CustomerAdmin"] = null;
            return RedirectWithMessage("successMessage", "Successfully logged out", "Index");
        }

        // Function to redirect with a tempdata message to show in the destination page (Its meant to reduce the lines of code)
        public ActionResult RedirectWithMessage(string type, string message, string view)
        {
            TempData[type] = message;
            return RedirectToAction(view);
        }

        // Function to check on incoming messages to display as alerts
        public void CheckMessages()
        {
            if (TempData["successMessage"] != null)
            {
                ViewBag.successMsg = TempData["successMessage"];
                TempData["successMessage"] = null;
            }

            if (TempData["errorMessage"] != null)
            {
                ViewBag.msg = TempData["errorMessage"];
                TempData["errorMessage"] = null;
            }
        }
    }
}