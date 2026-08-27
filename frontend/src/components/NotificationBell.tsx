import React, { useEffect, useState } from "react";
import { Badge, IconButton, Popover, List, ListItem, ListItemText, Typography, Button, Box } from "@mui/material";
import NotificationsIcon from "@mui/icons-material/Notifications";
import { useAppDispatch, useAppSelector } from "../store/hooks";
import { fetchUnreadCountThunk, fetchNotificationsThunk, markReadThunk } from "../store/notificationsSlice";

const NotificationBell: React.FC = () => {
  const dispatch = useAppDispatch();
  const { unreadCount, items } = useAppSelector((s) => s.notifications);
  const [anchor, setAnchor] = useState<HTMLElement | null>(null);

  useEffect(() => { dispatch(fetchUnreadCountThunk()); }, [dispatch]);

  const handleOpen = (e: React.MouseEvent<HTMLElement>) => {
    setAnchor(e.currentTarget);
    dispatch(fetchNotificationsThunk());
  };
  const handleMarkAllRead = () => {
    const unreadIds = items.filter(n => !n.isRead).map(n => n.id);
    if (unreadIds.length) dispatch(markReadThunk(unreadIds));
  };

  return (
    <>
      <IconButton data-testid="notification-bell" onClick={handleOpen} color="inherit">
        <Badge badgeContent={unreadCount} color="error" data-testid="unread-badge">
          <NotificationsIcon />
        </Badge>
      </IconButton>
      <Popover open={Boolean(anchor)} anchorEl={anchor} onClose={() => setAnchor(null)}
        anchorOrigin={{ vertical: "bottom", horizontal: "right" }}>
        <Box p={1} minWidth={320} data-testid="notification-popover">
          <Box display="flex" justifyContent="space-between" alignItems="center" px={1}>
            <Typography variant="subtitle1">Notifications</Typography>
            <Button size="small" onClick={handleMarkAllRead} data-testid="mark-all-read-btn">Mark all read</Button>
          </Box>
          {items.length === 0
            ? <Typography px={2} py={1} color="text.secondary" data-testid="no-notifications">No notifications.</Typography>
            : <List dense>
                {items.map(n => (
                  <ListItem key={n.id} data-testid={} sx={{ bgcolor: n.isRead ? "transparent" : "action.hover" }}>
                    <ListItemText primary={n.title} secondary={n.message} />
                  </ListItem>
                ))}
              </List>
          }
        </Box>
      </Popover>
    </>
  );
};
export default NotificationBell;
