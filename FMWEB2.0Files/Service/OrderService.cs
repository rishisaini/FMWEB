using AssetCare.Model.Models;
using AssetCare.Model.ViewModels;
using AssetCare.Repository.Interface;
using AssetCare.Service.Interfaces;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;

namespace AssetCare.Service
{
    public class OrderService : IOrderService
    {
        private IUnitOfWork _unitOfWork;
        private IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IOrderRepository orderRepository, IUserRepository userRepository, IMapper mapper)
        {
            this._unitOfWork = unitOfWork;
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public IEnumerable<Model.Models.Order> GetAll()
        {
            return _unitOfWork.OrderRepository.GetAll();
        }

        public PurchaseOrderDetailViewModel GetById(int orderId)
        {
            var getData = _unitOfWork.OrderRepository.GetById(orderId);
            var popData = _mapper.Map<PurchaseOrderDetailViewModel>(getData);

            return popData;
        }

        public void Update(PurchaseOrderDetailViewModel pmOrder)
        {
            var corePMJData = _unitOfWork.OrderRepository.GetById(pmOrder.OrderId);

            corePMJData.Code = pmOrder.Code;
            corePMJData.Description = pmOrder.Description;
            corePMJData.PropertyId = pmOrder.PropertyId;
            corePMJData.WorkCategoryId = pmOrder.WorkCategoryId;
            corePMJData.FinancialYearId = pmOrder.FinancialYearId;
            corePMJData.StartDate = pmOrder.StartDate;
            corePMJData.EndDate = pmOrder.EndDate;
            corePMJData.MIMSOrderNo = pmOrder.MIMSOrderNo;
            corePMJData.ContractorJobNo = pmOrder.ContractorJobNo;
            corePMJData.FeeJobNo = pmOrder.FeeJobNo;
            corePMJData.WorkSPNo = pmOrder.WorkSPNo;
            corePMJData.Approved = pmOrder.Approved;
            corePMJData.CommittedAmt = pmOrder.CommittedAmt;
            corePMJData.DefaultForJobType = pmOrder.DefaultForJobType;
            corePMJData.Completed = pmOrder.Completed;
            corePMJData.RecordedBy = pmOrder.RecordedBy;
            corePMJData.RecordedDate = pmOrder.RecordedDate;
            if (pmOrder.PropertyId == 0)
            {
                corePMJData.PropertyId = null;
            }
            else
            {
                corePMJData.PropertyId = pmOrder.PropertyId;
            }
            if (pmOrder.OrderJobTypeId == 0)
            {
                corePMJData.OrderJobTypeId = null;
            }
            else
            {
                corePMJData.OrderJobTypeId = pmOrder.OrderJobTypeId;
            }

            _unitOfWork.OrderRepository.Update(corePMJData);
            _unitOfWork.Save();
        }

        public int Add(PurchaseOrderDetailViewModel model)
        {
            var mappedData = _mapper.Map<AssetCare.Model.Models.Order>(model);

            _unitOfWork.OrderRepository.Insert(mappedData);

            if (mappedData.OrderJobTypeId == 0)
            {
                mappedData.OrderJobTypeId = null;
            }
            
            _unitOfWork.Save();
            return mappedData.OrderId;
        }

        public void AddLineItem(PurchaseOrderDetailViewModel model, string userCode) {
            OrderAdjustment orderAdjust = new OrderAdjustment();

            IEnumerable<OrderAdjustment> orderAdjustExisting = _unitOfWork.OrderAdjustmentRepository.GetAll().Where(x => x.OrderID == model.OrderId);

            orderAdjust.OrderID = model.OrderId;
            if (orderAdjustExisting.Count() != 0)
            {
                orderAdjust.DETOrderAdjustmentLineNo = (short?)(orderAdjustExisting.Max(x => x.DETOrderAdjustmentLineNo) == null ? 1 : orderAdjustExisting.Max(x => x.DETOrderAdjustmentLineNo) + 1);
            }
            else
            {
                orderAdjust.DETOrderAdjustmentLineNo = 1;
            }
            
            orderAdjust.DETOrderAdjustmentDate = model.AddOrderLineItem.DETOrderAdjustmentDate;
            orderAdjust.PrimarySchoolAmt = model.AddOrderLineItem.PrimarySchoolAmt;
            orderAdjust.SecondarySchoolAmt = model.AddOrderLineItem.SecondarySchoolAmt;
            orderAdjust.OtherAmt = model.AddOrderLineItem.OtherAmt;
            orderAdjust.FeeAmt = model.AddOrderLineItem.FeeAmt;
            orderAdjust.RecordedBy = userCode;
            orderAdjust.RecordedDate = System.DateTime.Now;

            _unitOfWork.OrderAdjustmentRepository.Insert(orderAdjust);
            _unitOfWork.Save();

        }

        public void UpdateLineItem(string[] liList, string userCode)
        {
            OrderAdjustment orderAdjust = _unitOfWork.OrderAdjustmentRepository.GetById(int.Parse(liList[0]));

            orderAdjust.PrimarySchoolAmt = decimal.Parse(liList[1]);
            orderAdjust.SecondarySchoolAmt = decimal.Parse(liList[2]);
            orderAdjust.FeeAmt = decimal.Parse(liList[3]);
            orderAdjust.OtherAmt = decimal.Parse(liList[4]);
            orderAdjust.RecordedBy = userCode;
            orderAdjust.RecordedDate = System.DateTime.Now;

            _unitOfWork.OrderAdjustmentRepository.Update(orderAdjust);
            _unitOfWork.Save();

        }

        public void Delete(Model.Models.Order order)
        {
            _unitOfWork.OrderRepository.Delete(order);
            _unitOfWork.Save();
        }

        public void Delete(int orderId)
        {
            _unitOfWork.OrderRepository.Delete(orderId);
            _unitOfWork.Save();
        }

        public IEnumerable<PurchaseOrderListViewModel> GetDataTableForPMPurchaseOrders(DataTableFilterModel filterModel, int agencyId, int finYearId, bool isPMOrder, out int filteredResultsCount, out int totalResultsCount)
        {
            var ignoreFields = new string[] { };
            return isPMOrder 
                ? _orderRepository.GetPMP(filterModel, agencyId, finYearId, (int)Consts.WorkCategory.PlannedMaintenanceProgram, ignoreFields, out filteredResultsCount, out totalResultsCount) 
                : _orderRepository.GetAllP(filterModel, agencyId, finYearId, ignoreFields, out filteredResultsCount, out totalResultsCount);
        }

        public IEnumerable<PurchaseOrderListViewModel> PurchaseOrdersList(DataTableFilterModel filterModel, int agencyId, int finYearId, int workCategoryID, out int filteredResultsCount, out int totalResultsCount)
        {
            var ignoreFields = new string[] { };
            return  _orderRepository.GetAll(filterModel, agencyId, finYearId, workCategoryID, ignoreFields,  out filteredResultsCount, out totalResultsCount);
        }

        public IEnumerable<PurchaseOrderLineItemListViewModel> GetDataTableForPMPurchaseOrderLIs(DataTableFilterModel filterModel, int orderId, out int filteredResultsCount, out int totalResultsCount)
        {
            var ignoreFields = new string[] { "OrderID", "OrderAdjustmentID" };
            return _orderRepository.GetLI(filterModel, orderId, ignoreFields, out filteredResultsCount, out totalResultsCount);
        }

        public int GetOrderJobTypeId(string code) {
            return _unitOfWork.OrderJobTypeRepository.GetAll().First(x => x.Code == code).OrderJobTypeId;
        }

        public IEnumerable<PurchaseOrderListViewModel> GetFeeOrders(int agencyId)
        {
            return _orderRepository.GetOrders(agencyId);
        }

        public decimal GetCommittedAmount(int orderId)
        {
            return _orderRepository.GetCommittedAmount(orderId);
        }

        public IEnumerable<KeyValuePair<int, string>> GetWorkCategoryLookup() {
            return _unitOfWork.WorkCategoryRepository.GetAll(x => x.InOrderList == true, q => q.OrderBy(y => y.Code)).Select(x => new KeyValuePair<int, string>(x.WorkCategoryId, x.Code + " - " + x.Description));
        }

        public IEnumerable<KeyValuePair<int, string>> GetOrderJobTypeLookup()
        {
            return _unitOfWork.OrderJobTypeRepository.GetAll(x => x.OrderJobTypeId != null, q => q.OrderBy(y => y.Code)).Select(x => new KeyValuePair<int, string>(x.OrderJobTypeId, x.Code + " - " + x.Description));
        }

        public void Dispose()
        {
            this._unitOfWork.Dispose();
        }
    }
}
