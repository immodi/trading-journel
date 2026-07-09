import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { RouterProvider } from 'react-router-dom'
import { router } from './router/routes'
import "./style.css";

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        <RouterProvider router={router} />;
    </StrictMode>,
)
