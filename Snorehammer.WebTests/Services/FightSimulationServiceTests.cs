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
        private AttackProfile weapon;
        private WeaponSimulation weaponSim;

        private FightSimulation sim;

        public virtual void Setup()
        {
            service = new FightSimulationService();
            weapon = new AttackProfile()
            {
                ArmorPenetration = 0,
                Attacks = 2,
                Damage = 1,
                Id = 1,
                Name = "bolt Rifle",
                Skill = 3,
                Strength = 4
            };
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

                    weapon
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
            weaponSim = new WeaponSimulation(weapon, unitProfile, 1);
            sim = new FightSimulation()
            {
                Attacker = attacker,
                Defender = unitProfile,
                WeaponSimulations = new List<WeaponSimulation>()
                {
                    weaponSim
                }
            };
        }
        public virtual void TearDown()
        {
            sim = null;
            attacker = null;
            unitProfile = null;
            service = null;
            weaponSim = null;
            weapon = null;
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
            public override void TearDown()
            {
                base.TearDown();
            }
            [Test]
            public void CalculateArmorSaveUnchanged()
            {
                //arrange
                //act
                service.DetermineArmorSave(weaponSim);
                //assert
                weaponSim.CoverIgnored.Should().BeFalse();
                weaponSim.ArmorSave.Should().Be(3);
            }
            [Test]
            public void CalculateArmorSaveMinusOne()
            {
                //arrange
                sim.Attacker.Attacks[0].ArmorPenetration = 1;
                //act
                service.DetermineArmorSave(weaponSim);
                //assert
                weaponSim.CoverIgnored.Should().BeFalse();
                weaponSim.ArmorSave.Should().Be(4);
            }
            [Test]
            public void CalculateArmorSaveUsesInvulnerable()
            {
                //arrange
                sim.Attacker.Attacks[0].ArmorPenetration = 4;
                sim.Defender.InvulnerableSave = 4;
                //act
                service.DetermineArmorSave(weaponSim);
                //assert
                weaponSim.CoverIgnored.Should().BeFalse();
                weaponSim.ArmorSave.Should().Be(4);
            }
            [Test]
            public void CalculateArmorSaveUsesCover()
            {
                //arrange
                sim.Attacker.Attacks[0].ArmorPenetration = 1;
                sim.Defender.HasCover = true;
                //act
                var res = service.DetermineArmorSave(weaponSim);
                //assert
                weaponSim.CoverIgnored.Should().BeFalse();
                res.Should().Be(3);
            }
            [Test]
            public void CalculateArmorSaveCoverDoesNotReduceTo2()
            {
                //arrange
                sim.Attacker.Attacks[0].ArmorPenetration = 0;
                sim.Defender.HasCover = true;
                //act
                var res = service.DetermineArmorSave(weaponSim);
                //assert
                weaponSim.CoverIgnored.Should().BeTrue();
                weaponSim.ArmorSave.Should().Be(3);
                res.Should().Be(3);
            }
            [Test]
            public void CalculateArmorSaveIgnoresCoverBecauseInvulnerable()
            {
                //arrange
                weaponSim.Weapon.ArmorPenetration = 3;
                weaponSim.Defender.InvulnerableSave = 4;
                weaponSim.Defender.HasCover = true;
                //act
                service.DetermineArmorSave(weaponSim);
                //assert
                weaponSim.CoverIgnored.Should().BeTrue();
                weaponSim.ArmorSave.Should().Be(4);
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
                    sim.Stats.DamageNumber = 20;
                    sim.Stats.PreFNPDamage = 20;
                    sim.Stats.ArmorSavesFailed = 20;
                    //act
                    service.DealDamage(sim);
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
                    sim.Stats.ArmorSavesFailed = 20;
                    sim.Stats.FeelNoPainMade = 1;
                    sim.Stats.PreFNPDamage = 20;
                    sim.Stats.DamageNumber = 19;
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
                    sim.Stats.PreFNPDamage = 20;
                    sim.Stats.FeelNoPainMade = 20;
                    sim.Stats.DamageNumber = 20;
                    sim.Stats.ArmorSavesFailed = 20;
                    //act
                    service.DealDamage(sim);
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
                    sim.Stats.DamageNumber = 20;
                    sim.Stats.ArmorSavesFailed = 20;
                    sim.Stats.PreFNPDamage = 20;
                    sim.Stats.ModelsDestroyed = 10;
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
                    sim.Stats.DamageNumber = 10;
                    sim.Stats.ArmorSavesFailed = 10;
                    //act
                    service.DealDamage(sim);
                    var res = service.GenerateWinnerMessage(sim);
                    //assert
                    res.Should().Contain("10 out of 20 attacks broke through armor.");
                    res.Should().Contain("10 wounds inflicted to defender.");
                    res.Should().Contain("5 out of 10 models were destroyed");
                    res.Should().NotContain("remaining");
                }
                [Test]
                public void WinnerMessageDamageButNoModelDestroyed()
                {
                    //arrange

                    A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(3);
                    A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).ReturnsNextFromSequence(1);
                    sim.WeaponSimulations[0].Weapon.Attacks = 20;
                    for (int i = 0; i < 20; i++)
                    {
                        diceList.Add(new Dice(unitProfile.MinimumSave, random));
                    }
                    sim.WeaponSimulations[0].ArmorDice = diceList;
                    sim.Stats.DamageNumber = 1;
                    sim.Stats.ArmorSavesFailed = 1;
                    //act
                    service.DealDamage(sim);
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
                    sim.Stats.DamageNumber = 1;
                    sim.Stats.ArmorSavesFailed = 1;

                    //act
                    service.DealDamage(sim);
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
                    sim.Stats.DamageNumber = 20;
                    sim.Stats.ArmorSavesFailed = 20;
                    //act
                    service.DealDamage(sim);
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
                    sim.Stats.DamageNumber = 0;
                    sim.Stats.ArmorSavesFailed = 0;
                    //act
                    service.DealDamage(sim);
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
                    sim.Stats.ArmorSavesFailed = sim.ArmorDice.Where(d => !d.Success).Count();
                    sim.Stats.DamageNumber = 5;
                    sim.Stats.WoundsInflicted = (int)sim.Stats.DamageNumber;
                    //act
                    service.DealDamage(sim);
                    var res = service.GenerateWinnerMessage(sim);
                    //assert
                    weaponSim.Stats.ModelsDestroyed.Should().Be(1);
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
                    sim.Stats.DamageNumber = 6;
                    sim.Stats.ArmorSavesFailed = sim.ArmorDice.Where(d => !d.Success).Count();
                    sim.Stats.FeelNoPainMade = sim.FeelNoPainDice.Where(d => d.Success).Count();
                    sim.Stats.DamageNumber -= sim.Stats.FeelNoPainMade;
                    sim.Stats.WoundsInflicted = sim.Stats.DamageNumber;
                    //act
                    service.DealDamage(sim);
                    var res = service.GenerateWinnerMessage(sim);
                    //assert
                    weaponSim.Stats.ModelsDestroyed.Should().Be(1);
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
                    sim.Stats.DamageNumber = 3;
                    sim.Stats.ArmorSavesFailed = sim.ArmorDice.Where(d => !d.Success).Count();
                    sim.Stats.WoundsInflicted = (int)sim.Stats.DamageNumber;
                    //act
                    service.DealDamage(sim);
                    var res = service.GenerateWinnerMessage(sim);
                    //assert
                    weaponSim.Stats.ModelsDestroyed.Should().Be(1);
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
                    sim.Stats.DamageNumber = 7;
                    sim.Stats.ArmorSavesFailed = sim.ArmorDice.Where(d => !d.Success).Count();
                    sim.Stats.FeelNoPainMade = sim.FeelNoPainDice.Where(d => d.Success).Count();
                    sim.Stats.DamageNumber -= sim.Stats.FeelNoPainMade;
                    sim.Stats.WoundsInflicted = sim.Stats.DamageNumber;
                    //act
                    service.DealDamage(sim);
                    var res = service.GenerateWinnerMessage(sim);
                    //assert
                    weaponSim.Stats.ModelsDestroyed.Should().Be(1);
                    weaponSim.Stats.WoundsInflicted.Should().Be(5);
                }
            }
        }
    }

}