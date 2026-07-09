import AuthPage from "@/pages/AuthPage";
import { createBrowserRouter } from "react-router-dom";


export const router = createBrowserRouter([
    {
        path: "/login",
        element: <AuthPage />,
    },
    // {
    //     path: "/",
    //     element: <Dashboard />,
    // },
]);
