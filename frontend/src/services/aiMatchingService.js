const runMatching = (axiosPrivate, orderId) => {
  return axiosPrivate.post(`/api/order/${orderId}/match`);
};

const getRisk = (axiosPrivate, orderId) => {
  return axiosPrivate.get(`/api/order/${orderId}/risk`);
};

const aiMatchingService = { runMatching, getRisk };

export default aiMatchingService;
