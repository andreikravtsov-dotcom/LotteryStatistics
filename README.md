# Eesti Loto Statistics Retriever

A lightweight .NET console application designed to fetch and save lottery draw statistics for **Bingo**, **Vikinglotto**, and **Eurojackpot** directly from the official Eesti Loto website, exporting them as structured JSON files.

## Features

* **Supported Lotteries:** Bingo, Vikinglotto, and Eurojackpot.
* **Automated Retrieval:** Connects to Eesti Loto to fetch the latest draw data.
* **JSON Export:** Automatically saves statistics as clean JSON files in your specified directory.
* **CLI Interface:** Simple command-line arguments for easy integration into automated scripts or manual execution.

## Prerequisites

* [.NET SDK](https://dotnet.microsoft.com/download) (compatible with your project's target framework)

## Building the Project

1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/lottery-statistics.git
   cd lottery-statistics
   ```

2. Build and publish the project using the .NET CLI:
   ```bash
   dotnet publish -c Release -o ./publish
   ```

## Usage

Run the compiled executable from the command line by providing the destination path for the files and the target lottery name.

```bash
LotteryStatistics.exe <SavePath> <LotteryName>
```

### Arguments

* `<SavePath>` — The target directory where the resulting JSON statistics files will be saved.
* `<LotteryName>` — The name of the lottery you wish to fetch. Supported values:
  * `bingo`
  * `viking`
  * `eurojackpot`

### Examples

```bash
# Fetch Bingo statistics and save them to C:\LotteryData
LotteryStatistics.exe "C:\LotteryData" bingo

# Fetch Eurojackpot statistics and save them to the current directory
LotteryStatistics.exe "." eurojackpot
```

## Output

The application outputs the retrieved draw statistics as JSON files directly into the specified `<SavePath>` directory.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
