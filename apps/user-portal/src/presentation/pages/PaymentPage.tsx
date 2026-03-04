/**
 * Страница оплаты (заглушка)
 */

import React from 'react';
import { Card, H2, Text, Icon } from '@blueprintjs/core';
import { Header } from '../components/layout/Header';

/**
 * Компонент PaymentPage
 * 
 * Заглушка для страницы оплаты. Отображает Header компонент 
 * и сообщение о том, что страница находится в разработке.
 * 
 * Требования: 7.3
 */
export const PaymentPage: React.FC = () => {
  return (
    <div className="payment-page">
      <Header />
      
      <div className="payment-content" style={{ padding: '20px' }}>
        <Card elevation={1} style={{ maxWidth: '600px', margin: '0 auto', textAlign: 'center' }}>
          <div style={{ marginBottom: '20px' }}>
            <Icon 
              icon="credit-card" 
              size={48} 
              style={{ color: '#5c7080', marginBottom: '15px' }} 
            />
          </div>
          
          <H2>Страница оплаты</H2>
          
          <Text style={{ fontSize: '16px', color: '#5c7080', marginTop: '15px' }}>
            Страница оплаты - в разработке
          </Text>
          
          <div 
            style={{ 
              marginTop: '30px', 
              padding: '20px', 
              backgroundColor: '#f5f8fa', 
              borderRadius: '6px' 
            }}
          >
            <Text style={{ color: '#5c7080' }}>
              Функционал оплаты будет добавлен в следующих версиях приложения.
              Здесь будут размещены формы оплаты, история платежей и управление подписками.
            </Text>
          </div>
        </Card>
      </div>
    </div>
  );
};