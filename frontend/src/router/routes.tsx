import { createBrowserRouter } from "react-router-dom";
import AuthPage from "@/pages/AuthPage";
import DashboardPage from "@/pages/DashboardPage";
import MarketsPage from "@/pages/MarketsPage";
import TradesPage from "@/pages/TradesPage";
import NotFoundPage from "@/pages/NotFoundPage";
import { AppLayout } from "@/components/layout/AppLayout";
import { ProtectedRoute } from "@/components/layout/ProtectedRoute";

export const router = createBrowserRouter([
    {
        path: "/login",
        element: <AuthPage />,
    },
    {
        element: <ProtectedRoute />,
        children: [
            {
                element: <AppLayout />,
                children: [
                    { index: true, element: <DashboardPage /> },
                    { path: "markets", element: <MarketsPage /> },
                    { path: "trades", element: <TradesPage /> },
                ],
            },
        ],
    },
    {
        path: "*",
        element: <NotFoundPage />,
    },
]);
