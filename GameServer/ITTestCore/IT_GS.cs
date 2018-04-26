using Application.Interfaces;
using Communication.RESTControllers;
using Domain.Interfaces;
using Domain.Models;
using Moq;
using NUnit.Framework;
using Persistance.UnitOfWork;

namespace IT_Core
{

    //[TestFixture] <--- Er blevet udkommenteret, da en textFixture uden tests får Travis til at fejle
    public class IT_Test
    {

        //UserController.LoginDto LoginInfo = new UserController.LoginDto();
        string user = "Username";
        string pass = "Pass";
        
        private IUser pers = new User();
        private Mock<IUnitOfWork> IU = new Mock<IUnitOfWork>();
        private Mock<ILoginManager> LM = new Mock<ILoginManager>();
        private Mock<IUserController> UC = new Mock<IUserController>();
        Mock<UnitOfWork> UOW = new Mock<UnitOfWork>(null);
        Mock<Communication.RESTControllers.UserController> mock = new Mock<UserController>();
        Mock<Application.Controllers.UserController> uc = new Mock<Application.Controllers.UserController>();

        [SetUp]
        public void Setup()
        {

            //UserController.LoginDto LoginInfo = new UserController.LoginDto();
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

        // DOES NOT RUN DO NOT RUN TEST!!!!



        //[Test]
        //public void IT_1_GS_PassesInfo()
        //{
        //    var commtest = mock;
        //    var res = uc;

        //    LoginInfo.Password = pass;
        //    LoginInfo.Username = user;
        //    commtest.Setup(m => m.GetUser(LoginInfo)).Verifiable();

        //    res.Setup(m => m.GetUser(pass, user));

        //    commtest.Verify(m => m.GetUser(LoginInfo));

        //}


        //[Test]
        //public void IT_2_GS_AppCreate()
        //{
     
        //    var UserCtrl = new Application.Controllers.UserController(IU.Object, LM.Object);

        //   // var SetsDB = UserCtrl.GetUser(user, pass).Returns(pers);

        //    var SetMethod = UserCtrl.CreateUser(pers);

        //    Assert.That(SetMethod, Is.EqualTo(pers));

        //}

        //[Test]
        //public void IT_3_GS_AppCreate()
        //{
        //    var A = new Mock<Application.Controllers.UserController>(IU,LM);
        //    A.Object.GetUser(user, pass);
        //    UOW.Verify(x => x.UserRepository.GetUserByUsername(It.IsAny<string>()),Times.Exactly(1));

        //}





    }
}