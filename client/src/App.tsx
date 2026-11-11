// client/src/App.tsx
import { Routes, Route, Navigate } from 'react-router-dom';

// Auth & Layout
import RequireAuth from './components/RequireAuth';
import RequireRole from './components/RequireRole';
import AppLayout from './components/AppLayout';

// Public
import LoginPage from './pages/public/LoginPage';
import RegisterPage from './pages/public/RegisterPage';

// User
import DashboardPage from './pages/user/DashboardPage';
import TransactionsPage from './pages/user/TransactionsPage';
import NewTransactionPage from './pages/user/NewTransactionPage';
import EditTransactionPage from './pages/user/EditTransactionPage';

// Operator
// 🔹 УДАЛЕНЫ: QueuePage, QueuePendingPage, QueueAssignedPage, QueueResolvedPage
import PendingOperationsPage from './pages/operator/PendingOperationsPage';

// Admin
import CategoriesPage from './pages/admin/CategoriesPage';
import UsersPage from './pages/admin/UsersPage';
import AdminDashboard from './pages/admin/AdminDashboard';

export default function App() {
  return (
    <Routes>
      {/* 🔹 Публичные маршруты */}
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      
      {/* 🔹 Защищённые маршруты (требуют авторизации) */}
      <Route element={<RequireAuth />}>
        <Route element={<AppLayout />}>
          
          {/* ===== USER РОЛИ (доступно всем авторизованным) ===== */}
          <Route element={<RequireRole allowedRoles={['User', 'Operator', 'Admin']} />}>
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/transactions" element={<TransactionsPage />} />
            <Route path="/transactions/new" element={<NewTransactionPage />} />
            <Route path="/transactions/:id/edit" element={<EditTransactionPage />} />
          </Route>
          
          {/* ===== OPERATOR РОЛИ ===== */}
          <Route element={<RequireRole allowedRoles={['Operator']} />}>
            {/* 🔹 Редиректы со старых маршрутов на новую страницу операций */}
            <Route path="/queue" element={<Navigate to="/operator/operations" replace />} />
            <Route path="/queue/new" element={<Navigate to="/operator/operations" replace />} />
            <Route path="/queue/assigned" element={<Navigate to="/operator/operations" replace />} />
            <Route path="/queue/resolved" element={<Navigate to="/operator/operations" replace />} />
            <Route path="/operator/queue/pending" element={<Navigate to="/operator/operations" replace />} />
            <Route path="/operator/queue/assigned" element={<Navigate to="/operator/operations" replace />} />
            <Route path="/operator/queue/resolved" element={<Navigate to="/operator/operations" replace />} />
            
            {/* 🔹 ЕДИНСТВЕННАЯ страница оператора: Ожидающие операции */}
            <Route path="/operator/operations" element={<PendingOperationsPage />} />
            
            {/* 🔹 Редирект с корня оператора */}
            <Route path="/operator" element={<Navigate to="/operator/operations" replace />} />
          </Route>
          
          {/* ===== ADMIN РОЛИ ===== */}
          <Route element={<RequireRole allowedRoles={['Admin']} />}>
            <Route path="/admin" element={<AdminDashboard />} />
            <Route path="/admin/categories" element={<CategoriesPage />} />
            <Route path="/admin/users" element={<UsersPage />} />
          </Route>
          
        </Route>
      </Route>
      
      {/* 🔹 404 - редирект на логин */}
      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  );
}