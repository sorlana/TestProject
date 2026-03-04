// Простой mock-сервер для демонстрации аутентификации
const express = require('express');
const cors = require('cors');
const app = express();

app.use(cors());
app.use(express.json());

// Демо пользователи
const users = [
  {
    id: '1',
    userName: 'admin',
    email: 'admin@testproject.com',
    phoneNumber: '+7 (999) 123-45-67',
    firstName: 'Администратор',
    lastName: 'Системы',
    middleName: 'Главный',
    emailConfirmed: true,
    phoneNumberConfirmed: true,
    role: 'admin'
  },
  {
    id: '2',
    userName: 'user',
    email: 'user@testproject.com',
    phoneNumber: '+7 (999) 765-43-21',
    firstName: 'Обычный',
    lastName: 'Пользователь',
    middleName: 'Тестовый',
    emailConfirmed: true,
    phoneNumberConfirmed: false,
    role: 'user'
  }
];

// Демо токены
const generateTokens = (user) => ({
  accessToken: `mock_access_token_${user.id}_${Date.now()}`,
  refreshToken: `mock_refresh_token_${user.id}_${Date.now()}`,
  expiresAt: new Date(Date.now() + 15 * 60 * 1000).toISOString() // 15 минут
});

// POST /auth/login
app.post('/auth/login', (req, res) => {
  const { userName, password, rememberMe } = req.body;
  
  console.log('Попытка входа:', { userName, password, rememberMe });
  
  // Демо логика: admin/admin123 или user/user123
  let user = null;
  if (userName === 'admin' && password === 'admin123') {
    user = users[0];
  } else if (userName === 'user' && password === 'user123') {
    user = users[1];
  }
  
  if (!user) {
    return res.status(401).json({
      success: false,
      errors: ['Неверный логин или пароль']
    });
  }
  
  const tokens = generateTokens(user);
  
  res.json({
    success: true,
    accessToken: tokens.accessToken,
    refreshToken: tokens.refreshToken,
    expiresAt: tokens.expiresAt,
    user: user
  });
});

// POST /auth/register
app.post('/auth/register', (req, res) => {
  const { userName, email, phoneNumber, password, firstName, lastName, middleName } = req.body;
  
  console.log('Регистрация:', { userName, email });
  
  // Проверяем, что пользователь не существует
  const existingUser = users.find(u => u.userName === userName || u.email === email);
  if (existingUser) {
    return res.status(400).json({
      success: false,
      errors: ['Пользователь с таким логином или email уже существует']
    });
  }
  
  // Создаем нового пользователя
  const newUser = {
    id: String(users.length + 1),
    userName,
    email,
    phoneNumber,
    firstName,
    lastName,
    middleName,
    emailConfirmed: false,
    phoneNumberConfirmed: false,
    role: 'user'
  };
  
  users.push(newUser);
  const tokens = generateTokens(newUser);
  
  res.json({
    success: true,
    accessToken: tokens.accessToken,
    refreshToken: tokens.refreshToken,
    expiresAt: tokens.expiresAt,
    user: newUser
  });
});

// POST /auth/google
app.post('/auth/google', (req, res) => {
  const { idToken } = req.body;
  
  console.log('Google OAuth:', { idToken });
  
  // Демо Google пользователь
  const googleUser = {
    id: '999',
    userName: 'google_user',
    email: 'google@testproject.com',
    phoneNumber: '',
    firstName: 'Google',
    lastName: 'User',
    middleName: '',
    emailConfirmed: true,
    phoneNumberConfirmed: false,
    role: 'user'
  };
  
  const tokens = generateTokens(googleUser);
  
  res.json({
    success: true,
    accessToken: tokens.accessToken,
    refreshToken: tokens.refreshToken,
    expiresAt: tokens.expiresAt,
    user: googleUser
  });
});

// POST /auth/refresh
app.post('/auth/refresh', (req, res) => {
  const { refreshToken } = req.body;
  
  console.log('Обновление токена:', { refreshToken });
  
  // Простая проверка refresh токена
  if (!refreshToken || !refreshToken.startsWith('mock_refresh_token_')) {
    return res.status(401).json({
      success: false,
      errors: ['Недействительный refresh токен']
    });
  }
  
  // Извлекаем ID пользователя из токена
  const userId = refreshToken.split('_')[3];
  const user = users.find(u => u.id === userId);
  
  if (!user) {
    return res.status(401).json({
      success: false,
      errors: ['Пользователь не найден']
    });
  }
  
  const tokens = generateTokens(user);
  
  res.json({
    success: true,
    accessToken: tokens.accessToken,
    refreshToken: tokens.refreshToken,
    expiresAt: tokens.expiresAt,
    user: user
  });
});

// POST /auth/logout
app.post('/auth/logout', (req, res) => {
  console.log('Выход из системы');
  
  res.json({
    success: true,
    message: 'Успешный выход из системы'
  });
});

// Проксирование остальных запросов на test-service
app.use('*', (req, res) => {
  res.status(404).json({
    error: 'Endpoint не найден в mock auth service'
  });
});

const PORT = 8082;
app.listen(PORT, () => {
  console.log(`Mock Auth Service запущен на порту ${PORT}`);
  console.log('Демо учетные записи:');
  console.log('  Администратор: admin / admin123');
  console.log('  Пользователь: user / user123');
});