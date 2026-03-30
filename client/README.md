# ServiceDesk Client (frontend)

## Назначение клиентской части
Клиентская часть реализует веб-интерфейс ServiceDesk на React + TypeScript: маршрутизацию, авторизацию через JWT, разграничение доступа по ролям и базовый каркас страниц.

## Что реально реализовано
- React 18 + TypeScript + Vite.
- UI на Ant Design.
- Маршрутизация на React Router v6.
- Подключен `@tanstack/react-query` (`QueryClientProvider` в `main.tsx`).
- `AuthContext` с инициализацией пользователя через `GET /api/auth/me`.
- Логин и регистрация через API:
  - `POST /api/auth/login`
  - `POST /api/auth/register`
- Хранение JWT в `localStorage` (`token`).
- Guards:
  - `RequireAuth` (редирект на `/login`, если не авторизован).
  - `RequireRole` (403-страница при недопустимой роли).
- Базовый layout (`AppLayout`) с меню, зависящим от роли.
- Страницы состояний: `PageLoading`, `PageError`, `PageEmpty`, `PageValidation`, `PageNotFound`, `ForbiddenPage`.
- `RootRedirect` для перенаправления с `/` по роли пользователя.

## Страницы

### Рабочие (есть реальная логика формы/доступа)
- `/login` (`LoginPage`) — форма входа, вызов API, обработка ошибок.
- `/register` (`RegisterPage`) — форма регистрации, вызов API, обработка ошибок.
- `/` (`RootRedirect`) — редирект на стартовую страницу по роли.
- `404` (`PageNotFound`) и `403` (`ForbiddenPage`) — рабочие служебные страницы.

### Пустые
- `/tickets` (`TicketsPage`)
- `/tickets/new` (`NewTicketPage`)
- `/tickets/:id` (`TicketDetailPage`)
- `/queue/new` (`QueueNewPage`)
- `/queue/assigned` (`QueueAssignedPage`)
- `/queue/resolved` (`QueueResolvedPage`)
- `/admin/categories` (`CategoriesPage`)
- `/admin/users` (`UsersPage`)

## Структура проекта
```text
client/
  src/
    api/
      client.ts            # fetch-клиент + ApiError + JWT header
    app/
      RootRedirect.tsx
    components/
      AppLayout.tsx
      RequireAuth.tsx
      RequireRole.tsx
      Page*.tsx
    contexts/
      AuthContext.tsx
    pages/
      LoginPage.tsx
      RegisterPage.tsx
      TicketDetailPage.tsx
      student/
      operator/
      admin/
    App.tsx                # маршруты приложения
    main.tsx               # провайдеры (QueryClient + Auth)
  vite.config.ts           # dev proxy /api -> http://localhost:5051
  README.md
```

## Требования для запуска
- Node.js и npm.
- Запущенный backend API на `http://localhost:5051` (Vite proxy для `/api`).

## Пошаговый локальный запуск
1. Перейти в клиент:
```bash
cd client
```
2. Установить зависимости:
```bash
npm install
```
3. Запустить dev-сервер:
```bash
npm run dev
```
4. Открыть в браузере:
- `http://localhost:5173`

## Переменные окружения
В текущем коде клиента переменные окружения (`VITE_*`) не используются.

API-база в `src/api/client.ts` задана как пустая строка:
```ts
const BASE_URL = '';
```
Запросы идут на относительные пути (`/api/...`), а в `vite.config.ts` настроен proxy:
- `/api` -> `http://localhost:5051`

## Текущее состояние интерфейса (честно)
- Рабочая часть интерфейса сейчас сосредоточена на auth-потоке (login/register/init/me) и разграничении доступа по ролям.
- Предметные страницы ServiceDesk (тикеты, очереди оператора, админ-разделы) пока являются заглушками без бизнес-логики и без данных.
- `react-query` подключен инфраструктурно, но на текущих страницах фактические query/mutation через него еще не реализованы.

## Примечание по архиву
В переданном архиве отсутствует `client/package.json`, поэтому команды `npm install` и `npm run dev` из этого README корректны для проекта как такового, но в текущей копии архива клиент без восстановления `package.json` не запустится.
