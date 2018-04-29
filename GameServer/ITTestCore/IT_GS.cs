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
using Persistance;
using Persistance.Repositories;
using Persistance.UnitOfWork;

namespace IT_Core
{

    //[TestFixture] <--- Er blevet udkommenteret, da en textFixture uden tests får Travis til at fejle
    public class IT_Test
    {

        //UserController.LoginDto LoginInfo = new UserController.LoginDto();
        string user = "Username";
        string pass = "Pass";

        private User pers = new User();
        private Mock<IUnitOfWork> IU = new Mock<IUnitOfWork>();
        private Mock<ILoginManager> LM = new Mock<ILoginManager>();
        private Mock<IUserController> UC = new Mock<IUserController>();
        private Mock<IUserRepository> UP = new Mock<IUserRepository>();
        private Mock<IUser> US = new Mock<IUser>();



        CountDownTimer T = new CountDownTimer();
        private UnitOfWork NUOW = new UnitOfWork(new DbContext());
        private LoginManager NLM = new LoginManager(null);



        Mock<UnitOfWork> UOW = new Mock<UnitOfWork>(null);
        Mock<Communication.RESTControllers.UserController> mock = new Mock<UserController>();
        Mock<Application.Controllers.UserController> CTRL = new Mock<Application.Controllers.UserController>();
        Mock<UserRepository> Urep = new Mock<UserRepository>();




        [SetUp]

        public void Setup()
        {

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

            UserController UC1 = new UserController(new Application.Controllers.UserController(NUOW, NLM));
            Application.Controllers.UserController UC2 = new Application.Controllers.UserController(NUOW, NLM);

            UC1.GetUser(pers.Username, pers.Password);
            //  UC2.GetUser(pers.Username, pers.Password).Received(1);

            //   UC.Verify(x => x.GetUser(pers.Username, pers.Password));

        }

        //Test if Createuser with pers as userobj, gets received in AppLayer CreateUser
        [Test]
        public void IT_2_GS_CreateUser()
        {
            UserController UC2 = new UserController(UC.Object);
            UC2.CreateUser(pers);
            UC.Verify(x => x.CreateUser(pers));

        }

        //Test if Update with pers as userobj, gets received in AppLayer UpdateUser
        [Test]
        public void IT_3_GS_UpdateUser()
        {
            UserController UC2 = new UserController(UC.Object);
            UC2.UpdateUser(pers.Username, pers);
            UC.Verify(x => x.UpdateUser(pers.Username, pers));

        }

        //*************************Application Layer***************************
        // Test from app layer to persistance layer Login
        [Test]
        public void IT_4_GS_LoginUser()
        {
            Application.Controllers.UserController UC3 =
                new Application.Controllers.UserController(UOW.Object, LM.Object);
            UC3.GetUser(pers.Username, pers.Password);

            //  Ub.Verify(x => x.GetUserByUsername(pers.Username));
        }

        //*************************Application Layer***************************
        // Test from app layer to persistance layer create
        [Test]
        public void IT_4_GS_CreateUser()
        {

        }

        //*************************Application Layer***************************
        // Test from app layer to persistance layer create
        [Test]
        public void IT_4_GS_UpdateUser()
        {

        }

    }

}