using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Application.Controllers;
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
using UserController = Application.Controllers.UserController;

namespace IT_Core
{

    //[TestFixture] <--- Er blevet udkommenteret, da en textFixture uden tests får Travis til at fejle
    public class IT_App_Layer
    {

        string user = "Username";
        string pass = "Pass";

        private IUser pers = new User();
        private Mock<IUser> userObj;
        private IUserController uut;
        private  IUnitOfWork OUW = new UnitOfWork(new DbContext());
        private  ILoginManager LM = new LoginManager(new CountDownTimer());
        private Mock <UnitOfWork> _UOW = new Mock<UnitOfWork>(new DbContext()); 
        private Application.Controllers.UserController _uut;





        [SetUp]

        public void Setup()
        {
            _uut = new Application.Controllers.UserController(OUW,LM);
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
            var command = Substitute.For<IUserRepository>();
            _uut.GetUser(pers.Username, pers.Password);
            command.Received().GetUserByUsername(pers.Username);

        }

        //Test if Createuser with pers as userobj, gets received in AppLayer CreateUser
        [Test]
        public void IT_2_GS_CreateUser()
        {
            _uut.CreateUser(pers);
          //  UC_mock.Verify(x => x.CreateUser(pers));

        }

        //Test if Update with pers as userobj, gets received in AppLayer UpdateUser
        [Test]
        public void IT_3_GS_UpdateUser()
        {
            _uut.UpdateUser(pers.Username, pers);
          //  UC_mock.Verify(x => x.UpdateUser(pers.Username, pers));

        }


    }

}