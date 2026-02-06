import { createTheme } from '@mui/material/styles';

export const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2', // Azul Material Design
    },
    secondary: {
      main: '#dc004e', // Rosa/Rojo
    },
  },
  typography: {
    fontFamily: 'Roboto, Arial, sans-serif',
  },
});
