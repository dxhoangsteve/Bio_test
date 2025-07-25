-- Fix CV_FilePath port from 7254 to 7255
UPDATE SiteConfigurations 
SET CV_FilePath = REPLACE(CV_FilePath, 'localhost:7254', 'localhost:7255')
WHERE CV_FilePath LIKE '%localhost:7254%';

-- Show updated result
SELECT ConfigID, Email, CV_FilePath FROM SiteConfigurations;
