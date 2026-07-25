const getSummary = (axiosPrivate) => {
  return axiosPrivate.get("/api/dashboard/summary");
};

const getAiComment = (axiosPrivate) => {
  return axiosPrivate.get("/api/dashboard/ai-comment");
};

const dashboardService = { getSummary, getAiComment };

export default dashboardService;
