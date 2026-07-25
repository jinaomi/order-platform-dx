const sendMessage = (axiosPrivate, message, history) => {
  return axiosPrivate.post("/api/chat/message", { message, history });
};

const chatService = { sendMessage };

export default chatService;
