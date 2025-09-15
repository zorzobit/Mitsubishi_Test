# Mitsubishi Robot Communication Client

A C# client library for direct socket communication with Mitsubishi MELFA industrial robots. This project provides a robust framework to connect, monitor, and control Mitsubishi robots, offering functionalities for reading robot status, positions, managing programs, and interacting with general-purpose I/O and user-defined variables.

This project is open-source and aims to provide a clear, extensible foundation for integrating Mitsubishi robots into .NET applications.

---

## Features

- **Connection Management:** Connect and disconnect from the Mitsubishi robot controller.
- **Robot State Retrieval:** Fetch robot state data, including positions in different coordinate systems (joint, world, user).
- **Robot Status Monitoring**:
    - Retrieve **robot override speed**.
    - Monitor **robot servo status** (via `GetState`).
    - Get **current program name** and **program execution status** (e.g., Running, Paused) (via `GetState`).
    - Read **current robot position** in both **joint coordinates (JPOSF)** and **Cartesian coordinates (PPOSF)**.
- **Robot Control**:
    - **Enable/Disable teach pendant operations** (`OperationEnabled`).
    - **Set robot override speed** (`Override`).
    - **Start, Pause, and Abort** robot program execution.
    - **Reset alarms** on the robot controller.
    - **Reset programs** and **initialize program slots**.
- **Variable Management**:
    - **Read and Write** user-defined **numeric variables** (e.g., `R_100`).
    - **Read and Write** user-defined **position variables** (e.g., `P_100`).
- **General Purpose I/O (DIO) Interaction**:
    - **Read the status of individual digital I/O bits** (Inputs and Outputs).
    - **Write (set/clear) the status of individual digital Output bits**, utilizing a safe read-modify-write approach to prevent unintended changes to other bits within the same 16-bit word.

---

## Getting Started

### Prerequisites

- **.NET Environment**: This project is built in C# and requires a .NET runtime (e.g., .NET 6, 7, or 8) to run.
- **Mitsubishi Robot Controller**: A physical or simulated Mitsubishi MELFA robot controller with network communication enabled.
- **Network Access**: Ensure your development machine has network access to the robot controller's IP address on the specified port (default 10001).

### Installation

1. Clone this repository to your local machine:
   ```bash
   git clone [https://github.com/zorzobit/Mitsubishi_Test.git](https://github.com/zorzobit/Mitsubishi_Test.git)
   ```
2. Open in Visual Studio (or your preferred IDE): Navigate to the cloned directory and open the `.sln` file.
3. Build the Project: Build the solution to restore NuGet packages and compile the project.

---

## Project Structure Highlights
The core logic resides within the MelfaClient class, which manages the TCP connection and provides methods for various robot interactions. Auxiliary classes like PositionP, PositionJ, State, and RDItem are used for structured data representation.

---

## Contributing
Contributions are welcome! If you find bugs, have suggestions for improvements, or want to add new features (e.g., more alarm details, input monitoring, file transfer), please feel free to:

1. Fork the repository.
2. Create a new branch (git checkout -b feature/your-feature-name).
3. Make your changes.
4. Commit your changes (git commit -m 'Add new feature').
5. Push to the branch (git push origin feature/your-feature-name).
6. Open a Pull Request.

---

## Acknowledgements
This project utilizes concepts and draws inspiration from the fantastic work done by [Palatis](https://github.com/palatis) in their [Melfa.Robot](https://github.com/Palatis/Melfa.Robot) project, particularly for understanding MELFA-BASIC socket messaging structures and some enum definitions. Their work provided a valuable starting point for this client.

## License
This project is licensed under the MIT License - see the LICENSE file for details.

<i>This ReadMe has been created with Gemini</i>
