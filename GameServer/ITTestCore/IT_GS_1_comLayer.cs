using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Application.Interfaces;
using Application.Managers;
using Application.Misc;
using Communication.RESTControllers;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.Azure.Documents.SystemFunctions;
using Moq;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Persistance;
using Persistance.Repositories;
using Persistance.UnitOfWork;

namespace IT_Core
{

    //[TestFixture] <--- Er blevet udkommenteret, da en textFixture uden tests får Travis til at fejle
    public class IT_CommLayer
    {

        //UserController.LoginDto LoginInfo = new UserController.LoginDto();
        string user = "Username";
        string pass = "Pass";

        private User pers = new User();
        private Mock<IUserController> UC_mock = new Mock<IUserController>();
        private UserController _uut;





        [SetUp]

        public void Setup()
        {
            _uut = new UserController(UC_mock.Object);
            string user = "UsernameIT";
            string givename = "GivennameIT";
            string last = "LastNameIT";
            string pass = "PasswordIT";
            string email = "dummy@dummy.dkIT";

            pers.Username = user;
            pers.GivenName = givename;
            pers.LastName = last;
            pers.Password = pass;
            pers.Email = email;


        }


        //*************************Test Communication Layer***************************

        //Test if we sent username and password to Usercontroller in communicationlayer, it gets received in usercontroler in applicationLayer
        [Test]
        public void IT_1_GS_LoginUser()
        {
            _uut.GetUser(pers.Username,pers.Password);
            UC_mock.Verify(x => x.GetUser(pers.Username,pers.Password));

        }

        //Test if Createuser with pers as userobj, gets received in AppLayer CreateUser
        [Test]
        public void IT_2_GS_CreateUser()
        {
            _uut.CreateUser(pers);
            UC_mock.Verify(x => x.CreateUser(pers));

        }

        //Test if Update with pers as userobj, gets received in AppLayer UpdateUser
        [Test]
        public void IT_3_GS_UpdateUser()
        {
            _uut.UpdateUser(pers.Username, pers);
            UC_mock.Verify(x => x.UpdateUser(pers.Username, pers));

        }


    }

}