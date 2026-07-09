import { Navigate, Route, Routes } from 'react-router-dom';
import AppLayout from '../components/layout/AppLayout';
import LoginPage from '../pages/auth/LoginPage';
import RegisterPage from '../pages/auth/RegisterPage';
import ForgotPasswordPage from '../pages/auth/ForgotPasswordPage';
import ChangePasswordPage from '../pages/auth/ChangePasswordPage';
import DashboardPage from '../pages/dashboard/DashboardPage';
import UsersPage from '../pages/users/UsersPage';
import DepartmentsPage from '../pages/referentials/DepartmentsPage';
import CommunesPage from '../pages/referentials/CommunesPage';
import HospitalsPage from '../pages/referentials/HospitalsPage';
import BirthRecordsPage from '../pages/birth-records/BirthRecordsPage';
import CreateBirthRecordPage from '../pages/birth-records/CreateBirthRecordPage';
import CertificateRequestsPage from '../pages/certificate-requests/CertificateRequestsPage';
import CertificatesPage from '../pages/certificates/CertificatesPage';
import CertificatePreviewPage from '../pages/certificates/CertificatePreviewPage';
import VerifyCertificatePage from '../pages/public/VerifyCertificatePage';
import { Roles } from '../utils/roles';
import ProtectedRoute from './ProtectedRoute';

export default function AppRoutes() {
  return (
    <Routes>
      <Route element={<LoginPage />} path="/login" />
      <Route element={<RegisterPage />} path="/register" />
      <Route element={<ForgotPasswordPage />} path="/forgot-password" />
      <Route
        element={
          <ProtectedRoute>
            <ChangePasswordPage />
          </ProtectedRoute>
        }
        path="/change-password"
      />
      <Route element={<VerifyCertificatePage />} path="/verify-certificate" />
      <Route
        element={
          <ProtectedRoute>
            <AppLayout />
          </ProtectedRoute>
        }
      >
        <Route element={<DashboardPage />} index />
        <Route
          element={
            <ProtectedRoute roles={[Roles.Administrateur]}>
              <UsersPage />
            </ProtectedRoute>
          }
          path="users"
        />
        <Route path="referentials">
          <Route
            element={
              <ProtectedRoute roles={[Roles.Administrateur]}>
                <DepartmentsPage />
              </ProtectedRoute>
            }
            path="departments"
          />
          <Route
            element={
              <ProtectedRoute roles={[Roles.Administrateur]}>
                <CommunesPage />
              </ProtectedRoute>
            }
            path="communes"
          />
          <Route
            element={
              <ProtectedRoute roles={[Roles.Administrateur]}>
                <HospitalsPage />
              </ProtectedRoute>
            }
            path="hospitals"
          />
        </Route>
        <Route path="birth-records">
          <Route
            element={
              <ProtectedRoute roles={[Roles.Administrateur, Roles.Medecin]}>
                <BirthRecordsPage />
              </ProtectedRoute>
            }
            index
          />
          <Route
            element={
              <ProtectedRoute roles={[Roles.Administrateur, Roles.AgentEtatCivil]}>
                <BirthRecordsPage mode="pending" />
              </ProtectedRoute>
            }
            path="pending-certificate"
          />
          <Route
            element={
              <ProtectedRoute roles={[Roles.Medecin]}>
                <CreateBirthRecordPage />
              </ProtectedRoute>
            }
            path="create"
          />
        </Route>
        <Route
          element={
            <ProtectedRoute roles={[Roles.Administrateur, Roles.AgentEtatCivil, Roles.Citoyen]}>
              <CertificateRequestsPage />
            </ProtectedRoute>
          }
          path="certificate-requests"
        />
        <Route
          element={
            <ProtectedRoute roles={[Roles.Administrateur, Roles.AgentEtatCivil, Roles.Citoyen]}>
              <CertificatesPage />
            </ProtectedRoute>
          }
          path="certificates"
        />
        <Route
          element={
            <ProtectedRoute roles={[Roles.Administrateur, Roles.AgentEtatCivil]}>
              <CertificatePreviewPage source="request" />
            </ProtectedRoute>
          }
          path="certificates/preview/from-request/:id"
        />
        <Route
          element={
            <ProtectedRoute roles={[Roles.Administrateur, Roles.AgentEtatCivil]}>
              <CertificatePreviewPage source="birth-record" />
            </ProtectedRoute>
          }
          path="certificates/preview/from-birth-record/:id"
        />
      </Route>
      <Route element={<Navigate replace to="/" />} path="*" />
    </Routes>
  );
}
