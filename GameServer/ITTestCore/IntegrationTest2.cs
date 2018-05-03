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
using UserController = Application.Controllers.UserController;

namespace IT_Core
{

    //[TestFixture] <--- Er blevet udkommenteret, da en textFixture uden tests får Travis til at fejle
    public class IntegrationTest2
    {
/*
        string user = "Username";
        string pass = "Pass";

        private User pers = new User();
        private Mock<IUnitOfWork> OUW = new Mock<IUnitOfWork>();
        private IUnitOfWork pOUW;
        private ILoginManager pLM;
        private Mock<ILoginManager> LM = new Mock<ILoginManager>();
        private Mock<UnitOfWork> OUWM = new Mock<UnitOfWork>();
        private Application.Controllers.UserController _uut;
*/





        [SetUp]
        public void Setup()
        {
            /*_uut = new UserController(pOUW,pLM);
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
*/

        }


        //*************************Test App Layer***************************

        //Test if we send usercontroler in applicationLayer gets received in persistance Layer
        [Test]
        public void IT_2_GS_LoginUser()
        {
          
        }

        //Test if Createuser with pers as userobj, gets received in persistance Layer
        [Test]
        public void IT_2_GS_CreateUser()
        {
            

        }



    }

}