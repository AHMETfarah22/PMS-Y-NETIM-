import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
});

export const getAvailableRooms = (start, end) =>
  api.get(`/reservation/available-rooms?start=${start}&end=${end}`);

export const getAvailableBeds = (roomNumber, start, end) =>
  api.get(`/reservation/available-beds/${roomNumber}?start=${start}&end=${end}`);

export const createBooking = (bookingData) =>
  api.post('/reservation/book', bookingData);

export const getCustomers = () =>
  api.get('/customer');

export const getCustomerDetails = (id) =>
  api.get(`/customer/${id}`);

export const updateCustomer = (id, data) =>
  api.put(`/customer/${id}`, data);

export const addCustomerMessage = (id, messageData) =>
  api.post(`/customer/${id}/messages`, messageData);

export default api;
