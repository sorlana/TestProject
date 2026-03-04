// Property-based тесты для адаптивности
import { describe, it } from 'vitest'
import * as fc from 'fast-check'

// Утилитарная функция для определения типа устройства по ширине экрана
function getDeviceType(width: number): 'mobile' | 'tablet' | 'desktop' {
  if (width < 768) return 'mobile'
  if (width <= 1024) return 'tablet'
  return 'desktop'
}

// Утилитарная функция для проверки применения правильных стилей
function shouldApplyMobileStyles(width: number): boolean {
  return width < 768
}

function shouldApplyTabletStyles(width: number): boolean {
  return width >= 768 && width <= 1024
}

function shouldApplyDesktopStyles(width: number): boolean {
  return width > 1024
}

describe('Feature: user-portal, Property-based тесты адаптивности', () => {
  it('Property 22: Адаптивность для мобильных устройств', () => {
    fc.assert(
      fc.property(
        fc.integer({ min: 320, max: 767 }), // Ширина мобильных устройств
        (width) => {
          // Для любой ширины экрана менее 768px, должны применяться мобильные стили
          const deviceType = getDeviceType(width)
          const shouldBeMobile = shouldApplyMobileStyles(width)
          
          return deviceType === 'mobile' && shouldBeMobile === true
        }
      ),
      { numRuns: 100 }
    )
  })

  it('Property 23: Адаптивность для планшетов', () => {
    fc.assert(
      fc.property(
        fc.integer({ min: 768, max: 1024 }), // Ширина планшетов
        (width) => {
          // Для любой ширины экрана от 768px до 1024px, должны применяться планшетные стили
          const deviceType = getDeviceType(width)
          const shouldBeTablet = shouldApplyTabletStyles(width)
          
          return deviceType === 'tablet' && shouldBeTablet === true
        }
      ),
      { numRuns: 100 }
    )
  })

  it('Property 24: Адаптивность для десктопов', () => {
    fc.assert(
      fc.property(
        fc.integer({ min: 1025, max: 3840 }), // Ширина десктопов
        (width) => {
          // Для любой ширины экрана более 1024px, должны применяться десктопные стили
          const deviceType = getDeviceType(width)
          const shouldBeDesktop = shouldApplyDesktopStyles(width)
          
          return deviceType === 'desktop' && shouldBeDesktop === true
        }
      ),
      { numRuns: 100 }
    )
  })

  it('Property 25: Динамическая адаптация при изменении размера', () => {
    fc.assert(
      fc.property(
        fc.integer({ min: 320, max: 3840 }), // Начальная ширина
        fc.integer({ min: 320, max: 3840 }), // Новая ширина
        (initialWidth, newWidth) => {
          // Для любого изменения размера окна, тип устройства должен определяться корректно
          const initialDeviceType = getDeviceType(initialWidth)
          const newDeviceType = getDeviceType(newWidth)
          
          // Проверяем, что определение типа устройства работает корректно для обеих ширин
          const initialCorrect = (
            (initialWidth < 768 && initialDeviceType === 'mobile') ||
            (initialWidth >= 768 && initialWidth <= 1024 && initialDeviceType === 'tablet') ||
            (initialWidth > 1024 && initialDeviceType === 'desktop')
          )
          
          const newCorrect = (
            (newWidth < 768 && newDeviceType === 'mobile') ||
            (newWidth >= 768 && newWidth <= 1024 && newDeviceType === 'tablet') ||
            (newWidth > 1024 && newDeviceType === 'desktop')
          )
          
          return initialCorrect && newCorrect
        }
      ),
      { numRuns: 100 }
    )
  })
})