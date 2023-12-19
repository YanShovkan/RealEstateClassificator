using RealEstateClassificator.Core.Services.Interfaces;
using RealEstateClassificator.Dal.Entities;
using RealEstateClassificator.Dal.Interfaces;
using Accord.Math;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using RealEstateClassificator.Common.Enums;

namespace RealEstateClassificator.Core.Services;

public class ClassificationService : IClassificationService
{
    private readonly ICommandRepository<Card> _classificationService;
    private readonly IUnitOfWork _unitOfWork;

    public ClassificationService(ICommandRepository<Card> commandRepository, IUnitOfWork unitOfWork)
    {
        _classificationService = commandRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task CalculateRealEstateClass()
    {
        var cards = await _classificationService.GetAllAsync();

        double[][] trainCards =
        [
            [7_500_000d, 3, 80, 65, 15, 2],
            [8_500_000d, 4, 120, 105, 15, 3],
            [10_300_000d, 5, 160, 145, 15, 3],
            [5_300_000d, 2, 65, 55, 10, 1],
            [6_800_000d, 1, 52, 40, 12, 2],
            [6_200_000d, 2, 60, 42, 18, 1],
            [4_300_000d, 0, 35, 25, 10, 0],
            [4_800_000d, 1, 40, 33, 7, 1],
            [3_200_000d, 0, 30, 22, 8, 1],
            [3_300_000d, 0, 35, 25, 10, 0],
            [2_800_000d, 1, 40, 33, 7, 1],
            [3_200_000d, 0, 30, 22, 8, 1]
        ];

        var trainOutputs = new[] {
            (int)PeopleGroups.Families,
            (int)PeopleGroups.Families,
            (int)PeopleGroups.Families,
            (int)PeopleGroups.Сouple,
            (int)PeopleGroups.Сouple,
            (int)PeopleGroups.Сouple,
            (int)PeopleGroups.Lonely,
            (int)PeopleGroups.Lonely,
            (int)PeopleGroups.Lonely,
            (int)PeopleGroups.Elderly,
            (int)PeopleGroups.Elderly,
            (int)PeopleGroups.Elderly
        };

        double[][] inputs = CardToArray(cards);

        var teacher = new MulticlassSupportVectorLearning<Gaussian>()
        {
            Learner = (param) => new SequentialMinimalOptimization<Gaussian>()
            {
                UseKernelEstimation = true
            }
        };

        var machine = teacher.Learn(trainCards, trainOutputs);

        int[] predicted = machine.Decide(inputs);

        int i = 0;
        foreach(var card in cards) 
        {
            card.ClassOfCard = predicted[i];
            i++;
        }
        await _unitOfWork.CommitAsync();
    }

    private double[][] CardToArray(IEnumerable<Card> cards)
    {
        var result = new double[cards.Count()][];

        int i = 0;

        foreach (var card in cards)
        {
            result[i] = [card.Price, (double)card.Rooms!, (double)card.TotalArea!, (double)card.LivingArea!, (double)card.KitchenArea!, (double)card.CargoLiftsCount! + (double)card.PassengerLiftsCount!];
            i++;
        }

        return result;
    }
}
