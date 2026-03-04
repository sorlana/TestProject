// Property-based тесты для валидации
import { describe, it } from 'vitest'
import * as fc from 'fast-check'
import { validateEmail, validatePassword, validatePasswordMatch } from '../../infrastructure/utils/validation'

describe('Feature: user-portal, Property-based тесты валидации', () => {
  it('Property 2: Email валидация', () => {
    // Настройка минимум 100 итераций
    fc.assert(
      fc.property(
        fc.emailAddress(),
        (validEmail) => {
          // Для любой валидной email строки, валидатор должен принимать её
          const result = validateEmail(validEmail)
          return result.isValid === true
        }
      ),
      { numRuns: 100 }
    )

    // Тест для невалидных email
    fc.assert(
      fc.property(
        fc.string().filter(s => !s.includes('@') || s.length < 3),
        (invalidEmail) => {
          // Для строк без @ или слишком коротких, валидатор должен отклонять
          const result = validateEmail(invalidEmail)
          return result.isValid === false
        }
      ),
      { numRuns: 100 }
    )
  })

  it('Property 3: Валидация минимальной длины пароля', () => {
    // Тест для коротких паролей (менее 8 символов)
    fc.assert(
      fc.property(
        fc.string({ maxLength: 7 }),
        (shortPassword) => {
          // Для любой строки короче 8 символов, валидатор должен отклонять
          const result = validatePassword(shortPassword)
          return result.isValid === false
        }
      ),
      { numRuns: 100 }
    )

    // Тест для достаточно длинных паролей (8+ символов)
    fc.assert(
      fc.property(
        fc.string({ minLength: 8, maxLength: 50 }),
        (longPassword) => {
          // Для любой строки длиной 8+ символов, валидатор должен принимать
          const result = validatePassword(longPassword)
          return result.isValid === true
        }
      ),
      { numRuns: 100 }
    )
  })

  it('Property 4: Валидация совпадения паролей', () => {
    // Тест для одинаковых паролей
    fc.assert(
      fc.property(
        fc.string({ minLength: 1, maxLength: 50 }),
        (password) => {
          // Для любой пары одинаковых строк, валидатор должен принимать
          const result = validatePasswordMatch(password, password)
          return result.isValid === true
        }
      ),
      { numRuns: 100 }
    )

    // Тест для разных паролей
    fc.assert(
      fc.property(
        fc.string({ minLength: 1, maxLength: 25 }),
        fc.string({ minLength: 1, maxLength: 25 }),
        (password1, password2) => {
          fc.pre(password1 !== password2) // Предусловие: пароли должны быть разными
          // Для любой пары разных строк, валидатор должен отклонять
          const result = validatePasswordMatch(password1, password2)
          return result.isValid === false
        }
      ),
      { numRuns: 100 }
    )
  })
})