# Fiware-databroker-console
Reads performance-data from Fiware, parses it and stores raw and parsed data in a database for visualisation with Superset on RAMP


Fiware-databroker-console is part of a project that shall allow user-controlled material transport within a workshop by using an Autonomous Mobile Robot (AMR).
In this repository the code for AMRControl is stored. The project relies on the FEATS-Project from Dalma-Systems, which can be found here: [FEATS](https://github.com/Dalma-Systems/FEATS)

This project is part of [DIH2](http://www.dih-squared.eu/). It also will be added to RAMP, see the [catalogue](https://github.com/ramp-eu).

This project is part of [FIWARE](https://www.fiware.org/). For more information check the [FIWARE Catalogue](https://github.com/Fiware/catalogue/).

# Build and install
- Assure that Fiware is running
- Start sending performance data to Fiware
- Clone the code of Fiware-databroker-console to a local directory
- Open it in Visual Studio or any other IDE supporting C#
- Create the database flexmill in a MariaDB-Instance
- Add the needed tables with the script that can be found here: [Generate tables](https://github.com/flexmill/Data-structures)
- Set the database-credentials in DBHelper.cs in lines 16-19
- Set the URL of you our Fiware-instance in API-Calls line 13
- Compile and run
- Data is collected on a regular basis and can be retrieved and visualized with superset
