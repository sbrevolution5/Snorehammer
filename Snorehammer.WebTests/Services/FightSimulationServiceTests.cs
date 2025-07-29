using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Snorehammer.Web.FrontendModels;
using Snorehammer.Web.FrontendModels.Profiles;
using Snorehammer.Web.FrontendModels.Simulations;

namespace Snorehammer.Web.Services.Tests
{
    public class FightSimulationServiceTests
    {
        private FightSimulationService service;
        private UnitProfile attacker;
        private UnitProfile unitProfile;

        private FightSimulation sim;

        public virtual void Setup()
        {
            service = new FightSimulationService();
            attacker = new UnitProfile()
            {
                Id = 1,
                InvulnerableSave = 0,
                ModelCount = 10,
                Name = "Intercessors",
                MinimumSave = 3,
                Toughness = 4,
                Wounds = 2,
                Attacks = new List<AttackProfile>() {

            new AttackProfile()
            {
                ArmorPenetration = 0,
                Attacks = 2,
                Damage = 1,
                Id = 1,
                Name = "bolt Rifle",
                Skill = 3,
                Strength = 4
            } 
                }
            };
            unitProfile = new UnitProfile()
            {
                Id = 1,
                InvulnerableSave = 0,
                ModelCount = 10,
                Name = "Intercessors",
                MinimumSave = 3,
                Toughness = 4,
                Wounds = 2
            };
            sim = new FightSimulation()
            {
                Attacker = attacker,
                Defender = unitProfile
            };
        }
        public virtual void TearDown()
        {
            sim = null;
            attacker = null;
            unitProfile = null;
            service = null;
        }
        [TestClass]
        public class FightSimulationArmorSaveTests : FightSimulationServiceTests
        {

            [SetUp]
            public override void Setup()
            {
                base.Setup();
            }
            [TearDown]
            public void TearDown()
            {
                base.TearDown();
            }
            [Test]
            public void CalculateArmorSaveUnchanged()
            {
                //arrange
                //act
                service.DetermineArmorSave(sim);
                //assert
                sim.CoverIgnored.Should().BeFalse();
                sim.ArmorSave.Should().Be(3);
            }
            [Test]
            public void CalculateArmorSaveMinusOne()
            {
                //arrange
                sim.Attacker.Attacks[0].ArmorPenetration = 1;
                //act
                service.DetermineArmorSave(sim);
                //assert
                sim.CoverIgnored.Should().BeFalse();
                sim.ArmorSave.Should().Be(4);
            }
            [Test]
            public void CalculateArmorSaveUsesInvulnerable()
            {
                //arrange
                sim.Attacker.Attacks[0].ArmorPenetration = 4;
                sim.Defender.InvulnerableSave = 4;
                //act
                service.DetermineArmorSave(sim);
                //assert
                sim.CoverIgnored.Should().BeFalse();
                sim.ArmorSave.Should().Be(4);
            }
            [Test]
            public void CalculateArmorSaveUsesCover()
            {
                //arrange
                sim.Attacker.Attacks[0].ArmorPenetration = 1;
                sim.Defender.HasCover = true;
                //act
                var res = service.DetermineArmorSave(sim);
                //assert
                sim.CoverIgnored.Should().BeFalse();
                res.Should().Be(3);
            }
            [Test]
            public void CalculateArmorSaveCoverDoesNotReduceTo2()
            {
                //arrange
                sim.Attacker.Attacks[0].ArmorPenetration = 0;
                sim.Defender.HasCover = true;
                //act
                var res = service.DetermineArmorSave(sim);
                //assert
                sim.CoverIgnored.Should().BeTrue();
                sim.ArmorSave.Should().Be(3);
                res.Should().Be(3);
            }
            [Test]
            public void CalculateArmorSaveIgnoresCoverBecauseInvulnerable()
            {
                //arrange
                sim.Attacker.Attacks[0].ArmorPenetration = 3;
                sim.Defender.InvulnerableSave = 4;
                sim.Defender.HasCover = true;
                //act
                service.DetermineArmorSave(sim);
                //assert
                sim.CoverIgnored.Should().BeTrue();
                sim.ArmorSave.Should().Be(4);
            }

            [TestClass]
            public class FightSimulationWinnerTests : FightSimulationServiceTests
            {
                private List<Dice> diceList;
                private List<Dice> fnpDiceList;
                private Random random = A.Fake<Random>();
                private Random fnpRandom = A.Fake<Random>();
                [SetUp]
                public override void Setup()
                {
                    base.Setup();
                    diceList = new List<Dice>();
                    fnpDiceList = new List<Dice>();
                }
                [TearDown]
                public override void TearDown()
                {
                    base.TearDown();
                    diceList = null;
                    fnpDiceList = null;
                }
                [Test]
                public void WinnerMessageFNPBlocksNone()
                {
                    //arrange
                    A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
                    unitProfile.FeelNoPain = true;
                    A.CallTo(() => fnpRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(1);
                    sim.Attacker.Attacks[0].Attacks = 20;
                    for (int i = 0; i < 20; i++)
                    {
                        diceList.Add(new Dice(unitProfile.MinimumSave, random));
                        fnpDiceList.Add(new Dice(unitProfile.FeelNoPainTarget, fnpRandom));
                    }
                    sim.ArmorDice = diceList;
                    sim.FeelNoPainDice = fnpDiceList;
                    sim.DamageNumber = 20;
                    //act
                    var res = service.GenerateWinnerMessage(sim);
                    //assert
                    res.Should().Contain("0 of 20 wounds blocked by Feel No Pain.");
                    res.Should().Contain("20 wounds inflicted to defender.");
                    res.Should().Contain("The entire unit was destroyed");
                }
                [Test]
                public void WinnerMessageFNPBlocksOne()
                {
                    //arrange
                    A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
                    unitProfile.FeelNoPain = true;
                    A.CallTo(() => fnpRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(1);
                    A.CallTo(() => fnpRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).ReturnsNextFromSequence(6);
                    sim.Attacker.Attacks[0].Attacks = 20;
                    for (int i = 0; i < 20; i++)
                    {
                        diceList.Add(new Dice(unitProfile.MinimumSave, random));
                        fnpDiceList.Add(new Dice(unitProfile.MinimumSave, fnpRandom));
                    }
                    sim.ArmorDice = diceList;
                    sim.FeelNoPainDice = fnpDiceList;
                    sim.DamageNumber = 20;
                    //act
                    var res = service.GenerateWinnerMessage(sim);
                    //assert
                    res.Should().Contain("20 out of 20 attacks broke through armor.");
                    res.Should().Contain("1 of 20 wounds blocked by Feel No Pain.");
                    res.Should().Contain("19 wounds inflicted to defender.");
                }
                [Test]
                public void WinnerMessageFNPBlocksAll()
                {
                    //arrange
                    A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
                    unitProfile.FeelNoPain = true;
                    A.CallTo(() => fnpRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(6);
                    sim.Attacker.Attacks[0].Attacks = 20;
                    for (int i = 0; i < 20; i++)
                    {
                        diceList.Add(new Dice(unitProfile.MinimumSave, random));
                        fnpDiceList.Add(new Dice(unitProfile.FeelNoPainTarget, fnpRandom));
                    }
                    sim.ArmorDice = diceList;
                    sim.FeelNoPainDice = fnpDiceList;
                    sim.DamageNumber = 20;
                    //act
                    var res = service.GenerateWinnerMessage(sim);
                    //assert
                    res.Should().Contain("20 out of 20 attacks broke through armor.");
                    res.Should().Contain("All wounds blocked by feel no pain. ");
                    res.Should().NotContain("20 wounds inflicted to defender.");
                    res.Should().NotContain("The entire unit was destroyed");
                }
                [Test]
                public void WinnerMessageUnitKilled()
                {
                    //arrange
                    A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
                    sim.Attacker.Attacks[0].Attacks = 20;
                    for (int i = 0; i < 20; i++)
                    {
                        diceList.Add(new Dice(unitProfile.MinimumSave, random));
                    }
                    sim.ArmorDice = diceList;
                    sim.DamageNumber = 20;
                    //act
                    var res = service.GenerateWinnerMessage(sim);
                    //assert
                    res.Should().Contain("20 out of 20 attacks broke through armor.");
                    res.Should().Contain("20 wounds inflicted to defender.");
                    res.Should().Contain("The entire unit was destroyed");
                }
                [Test]
                public void WinnerMessageHalfUnit()
                {
                    //arrange
                    var sequence = new List<int>();
                    for (int i = 0; i < 10; i++)
                    {
                        sequence.Add(2);
                        sequence.Add(3);
                    }
                    A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).ReturnsNextFromSequence(sequence.ToArray());
                    sim.Attacker.Attacks[0].Attacks = 20;
                    for (int i = 0; i < 20; i++)
                    {
                        diceList.Add(new Dice(unitProfile.MinimumSave, random));
                    }
                    sim.ArmorDice = diceList;
                    sim.DamageNumber = 10;
                    //act
                    var res = service.GenerateWinnerMessage(sim);
                    //assert
                    res.Should().Contain("10 out of 20 attacks broke through armor.");
                    res.Should().Contain("10 wounds inflicted to defender.");
                    res.Should().Contain("5 out of 10 models were destroyed");
                }
                [Test]
                public void WinnerMessageDamageButNoModelDestroyed()
                {
                    //arrange

                    A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(3);
                    A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).ReturnsNextFromSequence(1);
                    sim.Attacker.Attacks[0].Attacks = 20;
                    for (int i = 0; i < 20; i++)
                    {
                        diceList.Add(new Dice(unitProfile.MinimumSave, random));
                    }
                    sim.ArmorDice = diceList;
                    sim.DamageNumber = 1;
                    //act
                    var res = service.GenerateWinnerMessage(sim);
                    //assert
                    res.Should().Contain("1 out of 20 attacks broke through armor.");
                    res.Should().Contain("1 wounds inflicted to defender.");
                    res.Should().Contain("0 out of 10 models were destroyed");
                    res.Should().Contain("A remaining model was inflicted 1 wounds, leaving it with 1 remaining");
                }
                [Test]
                public void WinnerMessageDamagedSingleModel()
                {
                    //arrange

                    A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
                    sim.Attacker.Attacks[0].Attacks = 1;
                    unitProfile.ModelCount = 1;
                    diceList.Add(new Dice(unitProfile.MinimumSave, random));
                    sim.ArmorDice = diceList;
                    sim.DamageNumber = 1;

                    //act
                    var res = service.GenerateWinnerMessage(sim);
                    //assert
                    res.Should().Contain("1 out of 1 attacks broke through armor.");
                    res.Should().Contain("1 wounds inflicted to defender.");
                    res.Should().Contain("The model has 1 wound(s) remaining");
                }
                [Test]
                public void WinnerMessageDestroyedSingleModel()
                {
                    //arrange
                    A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(1);
                    sim.Attacker.Attacks[0].Attacks = 20;
                    unitProfile.ModelCount = 1;
                    for (int i = 0; i < 20; i++)
                    {
                        diceList.Add(new Dice(unitProfile.MinimumSave, random));
                    }
                    sim.ArmorDice = diceList;
                    sim.DamageNumber = 20;
                    //act
                    var res = service.GenerateWinnerMessage(sim);
                    //assert
                    res.Should().Contain("20 out of 20 attacks broke through armor.");
                    res.Should().Contain("20 wounds inflicted to defender.");
                    res.Should().Contain("The entire unit was destroyed");
                }
                [Test]
                public void WinnerMessage0Damage()
                {
                    //arrange
                    A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(3);
                    for (int i = 0; i < 2; i++)
                    {
                        diceList.Add(new Dice(unitProfile.MinimumSave, random));
                    }
                    sim.ArmorDice = diceList;
                    sim.DamageNumber = 0;
                    //act
                    var res = service.GenerateWinnerMessage(sim);
                    //assert
                    res.Should().Contain("0 out of 2 attacks broke through armor.");

                }
            }
            [TestClass]
            public class FightSimulationSpilloverTests : FightSimulationServiceTests
            {
                private List<Dice> diceList;
                private List<Dice> fnpDiceList;
                private List<Dice> woundDiceList;
                private Random random = A.Fake<Random>();
                private Random fnpRandom = A.Fake<Random>();
                private Random woundRandom = A.Fake<Random>();
                [SetUp]
                public override void Setup()
                {
                    base.Setup();
                    diceList = new List<Dice>();
                    fnpDiceList = new List<Dice>();
                    woundDiceList = new List<Dice>();
                }
                [TearDown]
                public override void TearDown()
                {
                    base.TearDown();
                    diceList = null;
                    fnpDiceList = null;
                    woundDiceList = null;
                }
                [Test]
                public void SpilloverDoesNotHappen()
                {
                    //arrange
                    sim.Attacker.Attacks[0].Damage = 5;
                    sim.Attacker.Attacks[0].Attacks = 1;
                    A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
                    for (int i = 0; i < sim.Attacker.Attacks[0].Attacks; i++)
                    {
                        diceList.Add(new Dice(unitProfile.MinimumSave, random));
                    }
                    sim.ArmorDice = diceList;
                    sim.DamageNumber = 5;
                    //act
                    var res = service.GenerateWinnerMessage(sim);
                    //assert
                    sim.Stats.ModelsDestroyed.Should().Be(1);
                }
                [Test]
                public void SpilloverUsesFeelNoPainRolls()
                {
                    //arrange
                    sim.Defender.Wounds = 3;
                    sim.Attacker.Attacks[0].Damage = 3;
                    sim.Attacker.Attacks[0].Attacks = 2;
                    A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
                    unitProfile.FeelNoPain = true;
                    A.CallTo(() => fnpRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(1);
                    //blocks 2 damage, lets 1 and 3 damage through, only killing a single model
                    A.CallTo(() => fnpRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).ReturnsNextFromSequence(6, 6);
                    for (int i = 0; i < sim.Attacker.Attacks[0].Attacks; i++)
                    {
                        diceList.Add(new Dice(unitProfile.MinimumSave, random));
                        fnpDiceList.Add(new Dice(unitProfile.FeelNoPainTarget, fnpRandom));
                    }
                    sim.ArmorDice = diceList;
                    sim.FeelNoPainDice = fnpDiceList;
                    sim.DamageNumber = 6;
                    //act
                    var res = service.GenerateWinnerMessage(sim);
                    //assert
                    sim.Stats.ModelsDestroyed.Should().Be(1);
                }
                [Test]
                public void SpilloverUsesVariableDamageRolls()
                {
                    //arrange
                    sim.Attacker.Attacks[0].IsVariableDamage = true;
                    sim.Attacker.Attacks[0].VariableAttackDiceNumber = 1;
                    sim.Attacker.Attacks[0].VariableDamageDiceSides = 6;
                    sim.Attacker.Attacks[0].Attacks = 2;
                    A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
                    A.CallTo(() => woundRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).ReturnsNextFromSequence(1, 2);
                    for (int i = 0; i < 2; i++)
                    {
                        diceList.Add(new Dice(unitProfile.MinimumSave, random));
                        woundDiceList.Add(new Dice(1, woundRandom));
                    }
                    sim.ArmorDice = diceList;
                    sim.WoundDice = woundDiceList;
                    sim.DamageNumber = 3;
                    //act
                    var res = service.GenerateWinnerMessage(sim);
                    //assert
                    sim.Stats.ModelsDestroyed.Should().Be(1);
                }
                [Test]
                public void SpilloverUsesVariableDamageAndFeelNoPainRolls()
                {
                    //arrange
                    sim.Attacker.Attacks[0].IsVariableDamage = true;
                    sim.Attacker.Attacks[0].VariableAttackDiceNumber = 1;
                    sim.Attacker.Attacks[0].VariableDamageDiceSides = 6;
                    sim.Attacker.Attacks[0].Attacks = 2;
                    sim.Defender.Wounds = 3;
                    A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
                    unitProfile.FeelNoPain = true;
                    A.CallTo(() => fnpRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(1);
                    //blocks 2 damage, lets 1 and 3 damage through, only killing a single model
                    A.CallTo(() => fnpRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).ReturnsNextFromSequence(6, 6);
                    A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
                    A.CallTo(() => woundRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).ReturnsNextFromSequence(1, 6);
                    for (int i = 0; i < 2; i++)
                    {
                        diceList.Add(new Dice(unitProfile.MinimumSave, random));
                        woundDiceList.Add(new Dice(1, woundRandom));
                    }
                    for (int i = 0; i < 7; i++)
                    {
                        fnpDiceList.Add(new Dice(6, fnpRandom));
                    }
                    sim.ArmorDice = diceList;
                    sim.FeelNoPainDice = fnpDiceList;
                    sim.WoundDice = woundDiceList;
                    sim.DamageNumber = 7;
                    //act
                    var res = service.GenerateWinnerMessage(sim);
                    //assert
                    sim.Stats.ModelsDestroyed.Should().Be(1);
                    sim.Stats.WoundsInflicted.Should().Be(5);
                }
            }
        }
    }

}