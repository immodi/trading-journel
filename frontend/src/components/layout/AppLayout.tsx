import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { LineChart, LayoutDashboard, ListOrdered, LogOut } from "lucide-react";
import { useAuth } from "@/hooks/useAuth";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { ThemeToggle } from "@/components/theme/ThemeToggle";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuGroup,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

const NAV_ITEMS = [
    { to: "/", label: "Dashboard", icon: LayoutDashboard, end: true },
    { to: "/trades", label: "Trades", icon: ListOrdered, end: false },
];

export function AppLayout() {
    const { user, logout } = useAuth();
    const navigate = useNavigate();

    function handleLogout() {
        logout();
        navigate("/login", { replace: true });
    }

    const initial = user?.username.charAt(0).toUpperCase() ?? "?";

    return (
        <div className="flex min-h-screen bg-muted/20">
            <aside className="sticky top-0 flex h-screen w-16 shrink-0 flex-col border-r bg-background md:w-60">
                <div className="flex h-14 items-center gap-2 px-3 md:px-4">
                    <span className="flex size-8 shrink-0 items-center justify-center rounded-lg bg-primary text-primary-foreground">
                        <LineChart className="size-4" />
                    </span>
                    <span className="hidden text-sm font-semibold tracking-tight md:block">
                        Trading Journal
                    </span>
                </div>

                <nav className="flex flex-1 flex-col gap-1 p-2">
                    {NAV_ITEMS.map(({ to, label, icon: Icon, end }) => (
                        <NavLink
                            key={to}
                            to={to}
                            end={end}
                            className={({ isActive }) =>
                                cn(
                                    "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors",
                                    isActive
                                        ? "bg-secondary text-secondary-foreground"
                                        : "text-muted-foreground hover:bg-muted hover:text-foreground"
                                )
                            }
                        >
                            <Icon className="size-4 shrink-0" />
                            <span className="hidden md:block">{label}</span>
                        </NavLink>
                    ))}
                </nav>

                <div className="border-t p-2">
                    <Button
                        variant="ghost"
                        className="w-full justify-start gap-3 px-3 text-muted-foreground"
                        onClick={handleLogout}
                    >
                        <LogOut className="size-4 shrink-0" />
                        <span className="hidden md:block">Sign out</span>
                    </Button>
                </div>
            </aside>

            <div className="flex min-w-0 flex-1 flex-col">
                <header className="sticky top-0 z-10 flex h-14 items-center justify-end gap-4 border-b bg-background/80 px-4 backdrop-blur md:px-6">
                    {/* <span className="text-sm font-medium text-muted-foreground"> */}
                    {/*     Trading Journal */}
                    {/* </span> */}
                    <div className="flex items-center gap-1">
                        <ThemeToggle />

                        <DropdownMenu>
                            <DropdownMenuTrigger
                                render={
                                    <Button variant="ghost" className="gap-2 px-2" />
                                }
                            >
                                <span className="flex size-7 items-center justify-center rounded-full bg-secondary text-xs font-semibold">
                                    {initial}
                                </span>
                                <span className="hidden text-sm sm:block">
                                    {user?.username}
                                </span>
                            </DropdownMenuTrigger>

                            <DropdownMenuContent align="end" className="w-56">
                                <DropdownMenuGroup>
                                    <DropdownMenuLabel className="text-foreground">
                                        <span className="block text-sm font-medium">
                                            {user?.username}
                                        </span>
                                        <span className="block text-xs font-normal text-muted-foreground">
                                            {user?.email}
                                        </span>
                                    </DropdownMenuLabel>
                                </DropdownMenuGroup>

                                <DropdownMenuSeparator />

                                <DropdownMenuItem variant="destructive" onClick={handleLogout}>
                                    <LogOut />
                                    Sign out
                                </DropdownMenuItem>
                            </DropdownMenuContent>
                        </DropdownMenu>
                    </div>
                </header>

                <main className="mx-auto w-full max-w-6xl flex-1 p-4 md:p-6">
                    <Outlet />
                </main>
            </div>
        </div>
    );
}
